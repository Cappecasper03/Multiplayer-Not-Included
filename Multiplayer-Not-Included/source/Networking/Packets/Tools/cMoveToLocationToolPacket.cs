using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cMoveToLocationToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private int      m_cell;
        private int      m_instance_id;

        public ePacketType m_type => ePacketType.kMoveToLocationTool;

        public cMoveToLocationToolPacket() {}

        public cMoveToLocationToolPacket( int _cell, int _instance_id )
        {
            m_cell        = _cell;
            m_instance_id = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_cell );
            _writer.Write( m_instance_id );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_cell        = _reader.ReadInt32();
            m_instance_id = _reader.ReadInt32();
        }

        public void onDispatched()
        {
            bool found = false;

            Navigator[] navigators = Object.FindObjectsOfType< Navigator >();
            foreach( Navigator navigator in navigators )
            {
                KPrefabID prefab_id = navigator?.GetComponent< KPrefabID >();
                if( prefab_id == null || prefab_id.InstanceID != m_instance_id )
                    continue;

                navigator.GetSMI< MoveToLocationMonitor.Instance >()?.MoveToLocation( m_cell );
                found = true;
                break;
            }

            if( !found )
            {
                Movable[] movables = Object.FindObjectsOfType< Movable >();
                foreach( Movable movable in movables )
                {
                    KPrefabID prefab_id = movable?.GetComponent< KPrefabID >();
                    if( prefab_id == null || prefab_id.InstanceID != m_instance_id )
                        continue;

                    movable.MoveToLocation( m_cell );
                    break;
                }
            }

            if( !cSession.isHost() )
                return;

            cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }
    }
}