using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using MultiplayerNotIncluded.source.Networking.Components;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public class cTreeFilterPacket : iIPacket
    {
        private enum eAction
        {
            kStatic,
            kDynamic,
        }

        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private eAction  m_action;
        private bool     m_add;
        private Tag      m_tag;
        private int      m_cell;
        private int      m_layer;
        private int      m_network_id;

        public ePacketType m_type => ePacketType.kTreeFilter;

        public static cTreeFilterPacket createStatic( bool _add, Tag _tag, int _cell, int _layer )
        {
            return new cTreeFilterPacket { m_action = eAction.kStatic, m_add = _add, m_tag = _tag, m_cell = _cell, m_layer = _layer };
        }

        public static cTreeFilterPacket createDynamic( bool _add, Tag _tag, int _network_id )
        {
            return new cTreeFilterPacket { m_action = eAction.kDynamic, m_add = _add, m_tag = _tag, m_network_id = _network_id };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_add );
            _writer.Write( m_tag.Name );

            switch( m_action )
            {
                case eAction.kStatic:
                {
                    _writer.Write( m_cell );
                    _writer.Write( m_layer );
                    break;
                }
                case eAction.kDynamic: _writer.Write( m_network_id ); break;
            }
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_action   = ( eAction )_reader.ReadInt32();
            m_add      = _reader.ReadBoolean();
            m_tag      = new Tag( _reader.ReadString() );

            switch( m_action )
            {
                case eAction.kStatic:
                {
                    m_cell  = _reader.ReadInt32();
                    m_layer = _reader.ReadInt32();
                    break;
                }
                case eAction.kDynamic: m_network_id = _reader.ReadInt32(); break;
            }
        }

        public void onReceived()
        {
            GameObject game_object = null;
            switch( m_action )
            {
                case eAction.kStatic: game_object = Grid.Objects[ m_cell, m_layer ]; break;
                case eAction.kDynamic:
                {
                    cNetworkIdentity identity;
                    if( cNetworkIdentity.tryGetIdentity( m_network_id, out identity ) )
                        game_object = identity.gameObject;
                    break;
                }
            }

            TreeFilterable tree_filterable = game_object?.GetComponent< TreeFilterable >();
            if( tree_filterable == null )
                return;

            cTreeFilterablePatch.s_skip_send = true;
            if( m_add )
                tree_filterable.AddTagToFilter( m_tag );
            else
                tree_filterable.RemoveTagFromFilter( m_tag );

            TreeFilterableSideScreen screen = Object.FindObjectOfType< TreeFilterableSideScreen >();
            if( screen != null )
            {
                Traverse traverse = Traverse.Create( screen ).Field( "visualDirty" );
                if( traverse != null )
                {
                    traverse.SetValue( true );
                    screen.Update();
                    traverse.SetValue( false );
                }
            }

            cTreeFilterablePatch.s_skip_send = false;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kStatic:  cLogger.logInfo( $"{_message}: {m_action}, {m_add}, {m_tag.Name}, {m_cell}, {( Grid.SceneLayer )m_layer}" ); break;
                case eAction.kDynamic: cLogger.logInfo( $"{_message}: {m_action}, {m_add}, {m_tag.Name}, {m_network_id}" ); break;
            }
        }
    }
}