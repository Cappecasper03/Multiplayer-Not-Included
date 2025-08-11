using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public abstract class cObjectMenuPacket< T > : iIPacket
        where T : KMonoBehaviour
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_value;
        private int      m_instance_id;

        public ePacketType m_type { get; }

        protected cObjectMenuPacket( ePacketType _type )
        {
            m_type = _type;
        }

        protected cObjectMenuPacket( ePacketType _type, bool _value, int _instance_id )
        {
            m_type        = _type;
            m_value       = _value;
            m_instance_id = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_value );
            _writer.Write( m_instance_id );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_value       = _reader.ReadBoolean();
            m_instance_id = _reader.ReadInt32();
        }

        public void onReceived()
        {
            T[] type_objects = Object.FindObjectsOfType< T >();
            foreach( T type_object in type_objects )
            {
                KPrefabID prefab_id = type_object.GetComponent< KPrefabID >();
                if( prefab_id == null || prefab_id.InstanceID != m_instance_id )
                    continue;

                onAction( m_value, type_object );

                Game.Instance.userMenu.Refresh( type_object.gameObject );
                break;
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_type}, {m_value}, {m_instance_id}" );

        protected abstract void onAction( bool _value, T _type_object );
    }
}