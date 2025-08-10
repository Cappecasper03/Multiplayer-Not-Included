using System.Collections.Generic;
using System.Linq;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class cSession
    {
        public static bool inSessionAndReady() => inSession()           && m_ready;
        public static bool inSession()         => cSteamLobby.inLobby() && s_connected_players.Count > 0;

        public static bool isHost()            => cSteamLobby.inLobby() && m_host_steam_id == m_local_steam_id;
        public static bool isClient()          => cSteamLobby.inLobby() && !isHost();
        public static bool isAllPlayersReady() => s_connected_players.All( _player => _player.Value.m_ready );

        public static bool m_ready = false;

        public static CSteamID m_host_steam_id  { get; private set; } = CSteamID.Nil;
        public static CSteamID m_local_steam_id => SteamUser.GetSteamID();

        public static readonly Dictionary< CSteamID, cPlayer > s_connected_players = new Dictionary< CSteamID, cPlayer >();

        public static bool tryGetPlayer( CSteamID _steam_id, out cPlayer _player ) => s_connected_players.TryGetValue( _steam_id, out _player );

        public static void removePlayer( CSteamID _steam_id )
        {
            cPlayer player;
            if( tryGetPlayer( _steam_id, out player ) )
                player.destroyCursor();

            s_connected_players.Remove( _steam_id );
        }

        public static cPlayer updateOrCreatePlayer( CSteamID _steam_id, HSteamNetConnection _connection )
        {
            cPlayer player = new cPlayer( _steam_id, _connection );
            s_connected_players[ _steam_id ] = player;

            tryGetPlayer( _steam_id, out player );
            return player;
        }

        public static void clear()
        {
            foreach( cPlayer player in s_connected_players.Values )
                player.destroyCursor();

            m_ready = false;
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