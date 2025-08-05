using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public class cMultiplayerPlayer
    {
        private CSteamID m_steam_id   { get; set; }
        private string   m_steam_name { get; set; }
        public  bool     isLocal      => m_steam_id == SteamUser.GetSteamID();

        public HSteamNetConnection m_connection   { get; set; } = HSteamNetConnection.Invalid;
        public bool                m_is_connected => m_connection != HSteamNetConnection.Invalid;

        public cMultiplayerPlayer( CSteamID _steam_id )
        {
            m_steam_id   = _steam_id;
            m_steam_name = SteamFriends.GetFriendPersonaName( _steam_id );
        }
    }
}