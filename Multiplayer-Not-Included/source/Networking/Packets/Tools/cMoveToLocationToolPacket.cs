using System;
using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.source.Networking.Components;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cMoveToLocationToolPacket : iIPacket
    {
        private enum eAction
        {
            kStatic,
            kDynamic,
        }

        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private eAction  m_action;
        private int      m_target_cell;
        private int      m_cell;
        private int      m_layer;
        private int      m_network_id;

        public ePacketType m_type => ePacketType.kMoveToLocationTool;

        public static cMoveToLocationToolPacket createStatic( int _target_cell, int _cell, int _layer )
        {
            return new cMoveToLocationToolPacket { m_action = eAction.kStatic, m_target_cell = _target_cell, m_cell = _cell, m_layer = _layer };
        }

        public static cMoveToLocationToolPacket createDynamic( int _target_cell, int _network_id )
        {
            return new cMoveToLocationToolPacket { m_action = eAction.kDynamic, m_target_cell = _target_cell, m_network_id = _network_id };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_target_cell );

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
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_action      = ( eAction )_reader.ReadInt32();
            m_target_cell = _reader.ReadInt32();

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

            if( game_object == null )
                return;

            Navigator                      navigator = game_object.GetComponent< Navigator >();
            MoveToLocationMonitor.Instance instance  = navigator?.GetSMI< MoveToLocationMonitor.Instance >();
            if( instance == null )
            {
                Movable movable = game_object.GetComponent< Movable >();
                if( movable == null )
                    return;

                movable.MoveToLocation( m_target_cell );
            }
            else
                instance.MoveToLocation( m_target_cell );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kStatic:  cLogger.logInfo( $"{_message}: {m_action}, {m_target_cell}, {m_cell}, {( Grid.SceneLayer )m_layer}" ); break;
                case eAction.kDynamic: cLogger.logInfo( $"{_message}: {m_action}, {m_target_cell}, {m_network_id}" ); break;
            }
        }
    }
}