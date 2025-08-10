using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Creatures;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Creatures
{
    public class cCaptureCreaturePacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_marked;
        private int      m_instance_id;

        public ePacketType m_type => ePacketType.kCaptureCreature;

        public cCaptureCreaturePacket() {}

        public cCaptureCreaturePacket( bool _marked, int _instance_id )
        {
            m_marked      = _marked;
            m_instance_id = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_marked );
            _writer.Write( m_instance_id );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_marked      = _reader.ReadBoolean();
            m_instance_id = _reader.ReadInt32();
        }

        public void onReceived()
        {
            Capturable[] capturables = Object.FindObjectsOfType< Capturable >();
            foreach( Capturable capturable in capturables )
            {
                KPrefabID prefab_id = capturable.GetComponent< KPrefabID >();
                if( prefab_id == null || prefab_id.InstanceID != m_instance_id )
                    continue;

                cCapturablePatch.s_skip_send = true;
                capturable.MarkForCapture( m_marked );
                cCapturablePatch.s_skip_send = false;
                break;
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: " );
    }
}