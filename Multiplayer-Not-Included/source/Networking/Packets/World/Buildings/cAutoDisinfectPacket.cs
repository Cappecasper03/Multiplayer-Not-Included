using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cAutoDisinfectPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_auto_disinfect;
        private int      m_instance_id;

        public ePacketType m_type => ePacketType.kAutoDisinfect;

        public cAutoDisinfectPacket() {}

        public cAutoDisinfectPacket( bool _auto_disinfect, int _instance_id )
        {
            m_auto_disinfect = _auto_disinfect;
            m_instance_id    = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_auto_disinfect );
            _writer.Write( m_instance_id );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id       = new CSteamID( _reader.ReadUInt64() );
            m_auto_disinfect = _reader.ReadBoolean();
            m_instance_id    = _reader.ReadInt32();
        }

        public void onReceived()
        {
            AutoDisinfectable[] auto_disinfectables = Object.FindObjectsOfType< AutoDisinfectable >();
            foreach( AutoDisinfectable auto_disinfectable in auto_disinfectables )
            {
                KPrefabID prefab_id = auto_disinfectable?.GetComponent< KPrefabID >();
                if( prefab_id == null || prefab_id.InstanceID != m_instance_id )
                    continue;

                cAutoDisinfectablePatch.s_skip_sending = true;
                if( m_auto_disinfect )
                    Traverse.Create( auto_disinfectable ).Method( "EnableAutoDisinfect" )?.GetValue();
                else
                    Traverse.Create( auto_disinfectable ).Method( "DisableAutoDisinfect" )?.GetValue();

                cAutoDisinfectablePatch.s_skip_sending = false;

                break;
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_auto_disinfect}, {m_instance_id}" );
    }
}