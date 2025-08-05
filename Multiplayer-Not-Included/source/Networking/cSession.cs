using System.Collections.Generic;
using System.Linq;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class cSession
    {
        public static CSteamID getLocalSteamID() => SteamUser.GetSteamID();
        public static bool     isHost()       => cSteamLobby.inLobby() && m_host_steam_id == getLocalSteamID();
        public static bool     isClient()     => cSteamLobby.inLobby() && !isHost();
        public static bool     isAllReady()   => s_connected_players.All( _player => _player.Value.m_ready );

        public static CSteamID m_host_steam_id { get; private set; } = CSteamID.Nil;

        public static readonly Dictionary< CSteamID, cPlayer > s_connected_players = new Dictionary< CSteamID, cPlayer >();

        public static void clear()
        {
            s_connected_players.Clear();
            m_host_steam_id = CSteamID.Nil;
            cLogger.logInfo( "Session cleared" );
        }

        public static void setHost( CSteamID _steam_id )
        {
            m_host_steam_id = _steam_id;
            cLogger.logInfo( $"Host ID: {m_host_steam_id}, Host: {isHost()}" );
        }
    }
}