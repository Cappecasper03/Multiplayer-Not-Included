using System;
using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.source.Networking.Components;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public abstract class cObjectMenuPacket< T > : iIPacket
        where T : KMonoBehaviour
    {
        protected enum eAction
        {
            kStatic,
            kDynamic,
        }

        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private eAction  m_action;
        private bool     m_value;
        private int      m_cell;
        private int      m_layer;
        private int      m_network_id;

        public ePacketType m_type { get; }

        protected cObjectMenuPacket( ePacketType _type )
        {
            m_type = _type;
        }

        protected cObjectMenuPacket( ePacketType _type, eAction _action, bool _value, int _cell, int _layer )
        {
            m_type   = _type;
            m_action = _action;
            m_value  = _value;
            m_cell   = _cell;
            m_layer  = _layer;
        }

        protected cObjectMenuPacket( ePacketType _type, eAction _action, bool _value, int _network_id )
        {
            m_type       = _type;
            m_action     = _action;
            m_value      = _value;
            m_network_id = _network_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_value );

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
            m_value    = _reader.ReadBoolean();

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

            T component = game_object?.GetComponent< T >();
            if( component == null )
                return;

            onAction( m_value, component );
            Game.Instance.userMenu.Refresh( component.gameObject );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kStatic:  cLogger.logInfo( $"{_message}: {m_type}, {m_action}, {m_value}, {m_cell}, {( Grid.SceneLayer )m_layer}" ); break;
                case eAction.kDynamic: cLogger.logInfo( $"{_message}: {m_type}, {m_action}, {m_value}, {m_network_id}" ); break;
            }
        }

        protected abstract void onAction( bool _value, T _type_object );
    }
}