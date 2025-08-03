using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class SteamRichPresence
    {
        public static void SetStatus( string status )
        {
            if( !SteamManager.Initialized )
            {
                DebugTools.Logger.LogError( "Steam Manager is not initialized" );
                return;
            }

            SteamFriends.ClearRichPresence();

            SteamFriends.SetRichPresence( "status", status );
            if( SteamLobby.InLobby )
            {
                DebugTools.Logger.LogInfo( "Updating lobby status" );
                SteamFriends.SetRichPresence( "steam_display",           "Lobby" );
                SteamFriends.SetRichPresence( "steam_player_group",      SteamLobby.CurrentLobbyID.ToString() );
                SteamFriends.SetRichPresence( "steam_player_group_size", $"{SteamMatchmaking.GetNumLobbyMembers( SteamLobby.CurrentLobbyID )}" );
            }

            DebugTools.Logger.LogInfo( $"Status set to: {status}" );
        }

        public static void ClearStatus()
        {
            if( !SteamManager.Initialized )
            {
                DebugTools.Logger.LogError( "Steam Manager is not initialized" );
                return;
            }

            SteamFriends.ClearRichPresence();
            DebugTools.Logger.LogInfo( "Status cleared" );
        }
    }
}