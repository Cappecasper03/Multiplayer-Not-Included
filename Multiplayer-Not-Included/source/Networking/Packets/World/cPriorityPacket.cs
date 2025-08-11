using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public class cPriorityPacket : iIPacket
    {
        private CSteamID        m_steam_id = cSession.m_local_steam_id;
        private PrioritySetting m_priority;
        private int             m_instance_id;

        public ePacketType m_type => ePacketType.kPriority;

        public cPriorityPacket() {}

        public cPriorityPacket( PrioritySetting _priority, int _instance_id )
        {
            m_priority    = _priority;
            m_instance_id = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_priority.priority_class );
            _writer.Write( m_priority.priority_value );
            _writer.Write( m_instance_id );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_priority    = new PrioritySetting( ( PriorityScreen.PriorityClass )_reader.ReadInt32(), _reader.ReadInt32() );
            m_instance_id = _reader.ReadInt32();
        }

        public void onReceived()
        {
            KPrefabID[] prefab_ids = Object.FindObjectsOfType< KPrefabID >();
            foreach( KPrefabID prefab_id in prefab_ids )
            {
                if( prefab_id.InstanceID != m_instance_id )
                    continue;

                Prioritizable prioritizable = prefab_id.GetComponent< Prioritizable >();
                prioritizable?.SetMasterPriority( m_priority );

                UserMenuScreen[] screens = Object.FindObjectsOfType< UserMenuScreen >();
                foreach( UserMenuScreen screen in screens )
                {
                    GameObject selected = Traverse.Create( screen ).Field( "selected" ).GetValue< GameObject >();
                    if( selected == null || selected != prefab_id.gameObject )
                        continue;

                    Traverse.Create( screen ).Method( "OnPriorityChanged", new[] { typeof( PrioritySetting ) } )?.GetValue( m_priority );
                    break;
                }

                break;
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_priority.priority_class}, {m_priority.priority_value}" );
    }
}