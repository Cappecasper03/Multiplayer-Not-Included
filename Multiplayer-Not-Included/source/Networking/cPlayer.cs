using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public class cPlayer
    {
        public bool isLocal     => m_steam_id   == SteamUser.GetSteamID();
        public bool isConnected => m_connection != HSteamNetConnection.Invalid;

        public HSteamNetConnection m_connection { get; set; }
        public string              m_steam_name { get; private set; }
        public bool                m_ready      { get; set; } = false;

        private CSteamID m_steam_id { get; set; }

        public cPlayer( CSteamID _steam_id, HSteamNetConnection _connection )
        {
            m_steam_id   = _steam_id;
            m_connection = _connection;
            m_steam_name = SteamFriends.GetFriendPersonaName( _steam_id );
        }
    }
}