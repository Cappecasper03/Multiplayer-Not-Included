using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class SteamRichPresence
    {
        public static void SetStatus( string status )
        {
            if( !SteamManager.Initialized )
            {
                Debug.LogError( "[SteamRichPresence.SetStatus] Steam Manager is not initialized" );
                return;
            }

            SteamFriends.ClearRichPresence();

            SteamFriends.SetRichPresence( "status", status );
            if( SteamLobby.InLobby )
            {
                Debug.Log( "[SteamRichPresence.SetStatus] Updating lobby status" );
                SteamFriends.SetRichPresence( "steam_display",           "Lobby" );
                SteamFriends.SetRichPresence( "steam_player_group",      SteamLobby.CurrentLobbyID.ToString() );
                SteamFriends.SetRichPresence( "steam_player_group_size", $"{SteamMatchmaking.GetNumLobbyMembers( SteamLobby.CurrentLobbyID )}" );
            }

            Debug.Log( $"[SteamRichPresence.SetStatus] Status set to: {status}" );
        }

        public static void ClearStatus()
        {
            if( !SteamManager.Initialized )
            {
                Debug.LogError( "[SteamRichPresence.ClearStatus] Steam Manager is not initialized" );
                return;
            }

            SteamFriends.ClearRichPresence();
            Debug.Log( "[SteamRichPresence.ClearStatus] Status cleared" );
        }
    }
}