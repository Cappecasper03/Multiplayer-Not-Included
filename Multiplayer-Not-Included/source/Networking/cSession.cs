using System.Collections.Generic;
using System.Linq;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class cSession
    {
        public static bool isHost()     => cSteamLobby.inLobby() && m_host_steam_id == m_local_steam_id;
        public static bool isClient()   => cSteamLobby.inLobby() && !isHost();
        public static bool isAllReady() => s_connected_players.All( _player => _player.Value.m_ready );

        public static CSteamID m_host_steam_id  { get; private set; } = CSteamID.Nil;
        public static CSteamID m_local_steam_id => SteamUser.GetSteamID();

        public static readonly Dictionary< CSteamID, cPlayer > s_connected_players = new Dictionary< CSteamID, cPlayer >();

        public static bool tryGetPlayer( CSteamID _steam_id, out cPlayer _player ) => s_connected_players.TryGetValue( _steam_id, out _player );
        public static bool removePlayer( CSteamID _steam_id ) => s_connected_players.Remove( _steam_id );

        public static cPlayer updateOrCreatePlayer( CSteamID _steam_id, HSteamNetConnection _connection )
        {
            cPlayer player;
            if( !tryGetPlayer( _steam_id, out player ) )
            {
                player                           = new cPlayer( _steam_id, _connection );
                s_connected_players[ _steam_id ] = player;
            }
            else
                player.m_connection = _connection;

            return player;
        }

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