using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Menus;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.Players
{
    public class cPlayerWaitPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private string   m_text;

        public ePacketType m_type => ePacketType.kPlayerWait;

        public cPlayerWaitPacket() {}
        public cPlayerWaitPacket( string _text ) => m_text = _text;

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_text );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_text     = _reader.ReadString();
        }

        public void onReceived()
        {
            if( !cSession.isClient() || m_steam_id != cSession.m_host_steam_id )
                return;

            if( !SpeedControlScreen.Instance.IsPaused )
                SpeedControlScreen.Instance.Pause( false );

            cMultiplayerLoadingOverlay.show( m_text );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_steam_id}" );
    }
}