using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class cSteamRichPresence
    {
        public static void setStatus( string _status )
        {
            if( !SteamManager.Initialized )
            {
                DebugTools.cLogger.logError( "Steam Manager is not initialized" );
                return;
            }

            SteamFriends.ClearRichPresence();

            SteamFriends.SetRichPresence( "status", _status );
            if( cSteamLobby.inLobby )
            {
                SteamFriends.SetRichPresence( "steam_display",           "Lobby" );
                SteamFriends.SetRichPresence( "steam_player_group",      cSteamLobby.m_current_lobby_id.ToString() );
                SteamFriends.SetRichPresence( "steam_player_group_size", $"{SteamMatchmaking.GetNumLobbyMembers( cSteamLobby.m_current_lobby_id )}" );
            }

            DebugTools.cLogger.logInfo( $"Status set to: {_status}" );
        }

        public static void clearStatus()
        {
            if( !SteamManager.Initialized )
            {
                DebugTools.cLogger.logError( "Steam Manager is not initialized" );
                return;
            }

            SteamFriends.ClearRichPresence();
            DebugTools.cLogger.logInfo( "Status cleared" );
        }
    }
}