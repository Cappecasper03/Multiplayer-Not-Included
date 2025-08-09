using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Menus;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.Players
{
    public class cPlayerReadyPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;

        public ePacketType m_type => ePacketType.kPlayerReady;

        public void serialize( BinaryWriter _writer ) => _writer.Write( m_steam_id.m_SteamID );

        public void deserialize( BinaryReader _reader ) => m_steam_id = new CSteamID( _reader.ReadUInt64() );

        public void onReceived()
        {
            if( cSession.isHost() )
            {
                cPlayer player;
                if( !cSession.tryGetPlayer( m_steam_id, out player ) )
                {
                    cLogger.logWarning( $"Packet sent from non-connected id: {m_steam_id}" );
                    return;
                }

                player.m_ready = true;
                cServer.setWaitingForPlayers();

                if( !cSession.isAllReady() )
                    return;

                cPacketSender.sendToAll( new cPlayerReadyPacket() );
            }
            else if( !cSession.isClient() || m_steam_id != cSession.m_host_steam_id )
                return;

            cMultiplayerLoadingOverlay.hide();
            cUtils.delayAction( 2000, () => cUtils.initializeUtility( "Power",       "Wire" ) );
            cUtils.delayAction( 2250, () => cUtils.initializeUtility( "Plumbing",    "LiquidConduit" ) );
            cUtils.delayAction( 2500, () => cUtils.initializeUtility( "Ventilation", "GasConduit" ) );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_steam_id}" );
    }
}