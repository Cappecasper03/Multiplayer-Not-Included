using System;
using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public class cSpeedChangePacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_is_paused;
        private int      m_speed;

        public ePacketType m_type => ePacketType.kSpeedChange;

        public cSpeedChangePacket() {}

        public cSpeedChangePacket( bool _is_paused, int _speed )
        {
            m_is_paused = _is_paused;
            m_speed     = _speed;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_is_paused );
            _writer.Write( m_speed );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id  = new CSteamID( _reader.ReadUInt64() );
            m_is_paused = _reader.ReadBoolean();
            m_speed     = _reader.ReadInt32();
        }

        public void onReceived()
        {
            cSpeedControlScreenPatch.s_skip_sending = true;
            switch( m_is_paused )
            {
                case true when !SpeedControlScreen.Instance.IsPaused: SpeedControlScreen.Instance.Pause(); break;
                case false when SpeedControlScreen.Instance.IsPaused: SpeedControlScreen.Instance.Unpause(); break;
            }

            SpeedControlScreen.Instance.SetSpeed( m_speed );
            cSpeedControlScreenPatch.s_skip_sending = false;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: IsPaused: {m_is_paused}, Speed: {m_speed}" );
    }
}