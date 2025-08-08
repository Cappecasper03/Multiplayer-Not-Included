using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.source.Networking.Components;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking
{
    public class cPlayer
    {
        public bool isLocal()     => m_steam_id   == SteamUser.GetSteamID();
        public bool isConnected() => m_connection != HSteamNetConnection.Invalid;

        public HSteamNetConnection m_connection { get; set; }
        public string              m_steam_name { get; private set; }
        public bool                m_ready      { get; set; } = false;

        public CSteamID m_steam_id { get; private set; }

        private cPlayerCursorComponent m_cursor { get; set; }

        public cPlayer( CSteamID _steam_id, HSteamNetConnection _connection )
        {
            m_steam_id   = _steam_id;
            m_connection = _connection;
            m_steam_name = SteamFriends.GetFriendPersonaName( _steam_id );

            if( m_steam_id == cSession.m_host_steam_id )
                m_ready = true;
        }

        public bool getOrCreateCursor( out cPlayerCursorComponent _cursor )
        {
            if( m_cursor != null )
            {
                _cursor = m_cursor;
                return true;
            }

            GameObject canvas = GameScreenManager.Instance?.ssCameraCanvas;
            if( canvas != null )
            {
                var cursor = new GameObject( $"Cursor_{m_steam_id}" );
                cursor.transform.SetParent( canvas.transform, false );
                cursor.layer = LayerMask.NameToLayer( "UI" );

                m_cursor = cursor.AddComponent< cPlayerCursorComponent >();
                m_cursor.initialize( m_steam_id );

                _cursor = m_cursor;
                return true;
            }

            cLogger.logWarning( "Cursor could not be created" );
            _cursor = null;
            return false;
        }

        public void destroyCursor()
        {
            if( m_cursor != null )
                Object.Destroy( m_cursor.gameObject );
            m_cursor = null;
        }
    }
}