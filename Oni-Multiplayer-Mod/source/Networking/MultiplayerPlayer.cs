using Steamworks;

namespace OniMultiplayerMod.Networking
{
    public class MultiplayerPlayer
    {
        private CSteamID SteamID   { get; set; }
        private string   SteamName { get; set; }
        public  bool     IsLocal   => SteamID == SteamUser.GetSteamID();

        public HSteamNetConnection? Connection  { get; set; } = null;
        public bool                 IsConnected => Connection.HasValue;

        public MultiplayerPlayer( CSteamID steamID )
        {
            SteamID   = steamID;
            SteamName = SteamFriends.GetFriendPersonaName( steamID );
        }
    }
}