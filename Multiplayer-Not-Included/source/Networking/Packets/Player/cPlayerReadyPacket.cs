using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Menus;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.Player
{
    public class cPlayerReadyPacket : iIPacket
    {
        private CSteamID m_steam_id;

        public ePacketType m_type => ePacketType.kPlayerReady;

        public cPlayerReadyPacket() {}

        public cPlayerReadyPacket( CSteamID _steam_id ) { m_steam_id = _steam_id; }

        public void serialize( BinaryWriter _writer ) { _writer.Write( m_steam_id.m_SteamID ); }

        public void deserialize( BinaryReader _reader ) { m_steam_id = new CSteamID( _reader.ReadUInt64() ); }

        public void onDispatched()
        {
            if( cSession.isHost )
            {
                cPlayer player;
                if( !cSession.s_connected_players.TryGetValue( m_steam_id, out player ) )
                {
                    cLogger.logWarning( $"Packet sent from non-connected id: {m_steam_id}" );
                    return;
                }

                player.m_ready = true;
                if( cSession.isAllReady )
                {
                    cServer.setWaitingForPlayers();
                    cPacketSender.sendToAll( new cPlayerReadyPacket( cSession.localSteamID ) );
                    cMultiplayerLoadingOverlay.hide();
                }
                else
                    cServer.setWaitingForPlayers();
            }
            else if( cSession.isClient && m_steam_id == cSession.m_host_steam_id )
                cMultiplayerLoadingOverlay.hide();
        }
    }
}