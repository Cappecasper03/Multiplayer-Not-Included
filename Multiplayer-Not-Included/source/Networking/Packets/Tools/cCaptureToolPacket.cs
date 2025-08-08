using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cCaptureToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private Vector2  m_min;
        private Vector2  m_max;

        public ePacketType m_type => ePacketType.kCaptureTool;

        public cCaptureToolPacket() {}

        public cCaptureToolPacket( Vector2 _min, Vector2 _max )
        {
            m_min = _min;
            m_max = _max;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_min );
            _writer.Write( m_max );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_min      = _reader.ReadVector2();
            m_max      = _reader.ReadVector2();
        }

        public void onReceived()
        {
            CaptureTool.MarkForCapture( m_min, m_max, true );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_min}, {m_max}" );
    }
}