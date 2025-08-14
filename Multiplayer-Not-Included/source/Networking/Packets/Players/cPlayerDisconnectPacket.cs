using System.IO;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.Players
{
    public class cPlayerDisconnectPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;

        public ePacketType m_type => ePacketType.kPlayerDisconnect;

        public void serialize( BinaryWriter _writer ) => _writer.Write( m_steam_id.m_SteamID );

        public void deserialize( BinaryReader _reader ) => m_steam_id = new CSteamID( _reader.ReadUInt64() );

        public void onReceived()
        {
            if( !cSession.isClient() || m_steam_id != cSession.m_host_steam_id )
                return;

            cSession.removePlayer( m_steam_id );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_steam_id}" );
    }
}