using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public class cPlayer
    {
        public bool isLocal     => m_steam_id   == SteamUser.GetSteamID();
        public bool isConnected => m_connection != HSteamNetConnection.Invalid;

        public HSteamNetConnection m_connection { get; set; } = HSteamNetConnection.Invalid;

        private CSteamID m_steam_id   { get; set; }
        private string   m_steam_name { get; set; }

        public cPlayer( CSteamID _steam_id )
        {
            m_steam_id   = _steam_id;
            m_steam_name = SteamFriends.GetFriendPersonaName( _steam_id );
        }
    }
}