using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cDeconstructPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_cancel;
        private int      m_instance_id;

        public ePacketType m_type => ePacketType.kDeconstruct;

        public cDeconstructPacket() {}

        public cDeconstructPacket( bool _cancel, int _instance_id )
        {
            m_cancel      = _cancel;
            m_instance_id = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_cancel );
            _writer.Write( m_instance_id );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_cancel      = _reader.ReadBoolean();
            m_instance_id = _reader.ReadInt32();
        }

        public void onReceived()
        {
            Deconstructable[] deconstructables = Object.FindObjectsOfType< Deconstructable >();
            foreach( Deconstructable deconstructable in deconstructables )
            {
                KPrefabID prefab_id = deconstructable?.GetComponent< KPrefabID >();
                if( prefab_id == null || prefab_id.InstanceID != m_instance_id )
                    continue;

                if( m_cancel )
                    deconstructable.CancelDeconstruction();
                else
                    deconstructable.QueueDeconstruction( true );
                break;
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_steam_id}" );
    }
}