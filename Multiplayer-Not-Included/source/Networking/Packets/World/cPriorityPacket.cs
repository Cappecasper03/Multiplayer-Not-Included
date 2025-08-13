using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.source.Networking.Components;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public class cPriorityPacket : iIPacket
    {
        private enum eAction
        {
            kStatic,
            kDynamic,
        }

        private CSteamID        m_steam_id = cSession.m_local_steam_id;
        private eAction         m_action;
        private PrioritySetting m_priority;
        private int             m_cell;
        private int             m_layer;
        private int             m_network_id;

        public ePacketType m_type => ePacketType.kPriority;

        public static cPriorityPacket createStatic( PrioritySetting _priority, int _cell, int _layer )
        {
            return new cPriorityPacket { m_action = eAction.kStatic, m_priority = _priority, m_cell = _cell, m_layer = _layer };
        }

        public static cPriorityPacket createDynamic( PrioritySetting _priority, int _network_id )
        {
            return new cPriorityPacket { m_action = eAction.kDynamic, m_priority = _priority, m_network_id = _network_id };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( ( int )m_priority.priority_class );
            _writer.Write( m_priority.priority_value );

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
            m_priority = new PrioritySetting( ( PriorityScreen.PriorityClass )_reader.ReadInt32(), _reader.ReadInt32() );

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
                    if( !cNetworkIdentity.tryGetIdentity( m_network_id, out identity ) )
                        return;

                    game_object = identity.gameObject;
                    break;
                }
            }

            if( game_object == null )
                return;

            Prioritizable prioritizable = game_object.GetComponent< Prioritizable >();
            prioritizable?.SetMasterPriority( m_priority );

            UserMenuScreen screen   = Object.FindObjectOfType< UserMenuScreen >();
            GameObject     selected = Traverse.Create( screen ).Field( "selected" )?.GetValue< GameObject >();
            if( selected == game_object )
                Traverse.Create( screen ).Method( "OnPriorityChanged", new[] { typeof( PrioritySetting ) } )?.GetValue( m_priority );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kStatic:  cLogger.logInfo( $"{_message}: {m_action}, {m_priority.priority_class}, {m_priority.priority_value}, {m_cell}, {( Grid.SceneLayer )m_layer}" ); break;
                case eAction.kDynamic: cLogger.logInfo( $"{_message}: {m_action}, {m_priority.priority_class}, {m_priority.priority_value}, {m_network_id}" ); break;
            }
        }
    }
}