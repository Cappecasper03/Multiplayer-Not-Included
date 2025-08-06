using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.Tool;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cPrioritizeToolPacket : iIPacket
    {
        private CSteamID        m_steam_id = cSession.m_local_steam_id;
        private int             m_instance_id;
        private PrioritySetting m_priority;

        public ePacketType m_type => ePacketType.kPrioritizeTool;

        public cPrioritizeToolPacket() {}

        public cPrioritizeToolPacket( int _instance_id, PrioritySetting _priority )
        {
            m_instance_id = _instance_id;
            m_priority    = _priority;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_instance_id );
            _writer.Write( ( int )m_priority.priority_class );
            _writer.Write( m_priority.priority_value );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_instance_id = _reader.ReadInt32();
            m_priority    = new PrioritySetting( ( PriorityScreen.PriorityClass )_reader.ReadInt32(), _reader.ReadInt32() );
        }

        public void onReceived()
        {
            Prioritizable[] prioritizables = Object.FindObjectsOfType< Prioritizable >();
            foreach( Prioritizable prioritizable in prioritizables )
            {
                KPrefabID prefab_id = prioritizable?.GetComponent< KPrefabID >();
                if( prefab_id == null || prefab_id.InstanceID != m_instance_id )
                    continue;

                cPrioritizeToolPatch.s_skip_sending = true;
                MethodInfo try_prioritize_game_object = PrioritizeTool.Instance.GetType().GetMethod( "TryPrioritizeGameObject", BindingFlags.NonPublic | BindingFlags.Instance );
                try_prioritize_game_object?.Invoke( PrioritizeTool.Instance, new object[] { prioritizable.gameObject, m_priority } );
                cPrioritizeToolPatch.s_skip_sending = false;
                break;
            }

            if( !cSession.isHost() )
                return;

            cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_steam_id}" );
    }
}