using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cAutoRepairPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_auto_repair;
        private int      m_instance_id;

        public ePacketType m_type => ePacketType.kAutoRepair;

        public cAutoRepairPacket() {}

        public cAutoRepairPacket( bool _auto_repair, int _instance_id )
        {
            m_auto_repair = _auto_repair;
            m_instance_id = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_auto_repair );
            _writer.Write( m_instance_id );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_auto_repair = _reader.ReadBoolean();
            m_instance_id = _reader.ReadInt32();
        }

        public void onReceived()
        {
            Repairable[] repairables = Object.FindObjectsOfType< Repairable >();
            foreach( Repairable repairable in repairables )
            {
                KPrefabID prefab_id = repairable?.GetComponent< KPrefabID >();
                if( prefab_id == null || prefab_id.InstanceID != m_instance_id )
                    continue;

                cRepairablePatch.s_skip_sending = true;
                if( m_auto_repair )
                {
                    MethodInfo allow_repair = repairable.GetType().GetMethod( "AllowRepair", BindingFlags.NonPublic | BindingFlags.Instance );
                    allow_repair?.Invoke( repairable, new object[] {} );
                }
                else
                    repairable.CancelRepair();

                cRepairablePatch.s_skip_sending = false;

                break;
            }

            if( !cSession.isHost() )
                return;

            cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_steam_id}" );
    }
}