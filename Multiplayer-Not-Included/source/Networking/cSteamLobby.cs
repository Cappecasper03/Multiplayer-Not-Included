using MultiplayerNotIncluded.DebugTools;
using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class cSteamLobby
    {
        public static bool inLobby() => m_lobby_id != CSteamID.Nil;

        private static CSteamID m_lobby_id       { get; set; } = CSteamID.Nil;
        private static int      m_max_lobby_size { get; set; } = 4;

        private static Callback< LobbyCreated_t >           s_on_created;
        private static Callback< GameLobbyJoinRequested_t > s_on_join_requested;
        private static Callback< LobbyEnter_t >             s_on_entered;

        public static void initialize()
        {
            if( !SteamManager.Initialized )
            {
                cLogger.logError( "Steam Manager is not initialized" );
                return;
            }

            s_on_created        = Callback< LobbyCreated_t >.Create( onCreated );
            s_on_join_requested = Callback< GameLobbyJoinRequested_t >.Create( onJoinRequested );
            s_on_entered        = Callback< LobbyEnter_t >.Create( onEntered );

            cLogger.logInfo( "Callbacks registered" );
        }

        public static void create( ELobbyType _lobby_type = ELobbyType.k_ELobbyTypeFriendsOnly )
        {
            if( !SteamManager.Initialized )
                return;

            if( inLobby() )
            {
                cLogger.logInfo( "Already in another lobby, leaving current lobby" );
                leave();
            }

            SteamMatchmaking.CreateLobby( _lobby_type, m_max_lobby_size );
        }

        private static void join( CSteamID _lobby_id )
        {
            if( !SteamManager.Initialized )
                return;

            if( inLobby() )
            {
                cLogger.logInfo( "Already in another lobby, leaving current lobby" );
                leave();
            }

            cLogger.logInfo( $"Joining lobby: {_lobby_id}" );
            SteamMatchmaking.JoinLobby( _lobby_id );
        }

        public static void leave()
        {
            if( !SteamManager.Initialized )
                return;

            if( !inLobby() )
                return;

            cClient.disconnect();
            cServer.stop();
            cSession.clear();

            SteamMatchmaking.LeaveLobby( m_lobby_id );
            cLogger.logInfo( $"Left lobby: {m_lobby_id}" );
            m_lobby_id = CSteamID.Nil;
        }

        private static void onCreated( LobbyCreated_t _data )
        {
            if( _data.m_eResult != EResult.k_EResultOK )
            {
                cLogger.logError( "Failed to create lobby" );
                return;
            }

            m_lobby_id = new CSteamID( _data.m_ulSteamIDLobby );
            cLogger.logInfo( $"Lobby created: {m_lobby_id}" );

            SteamMatchmaking.SetLobbyData( m_lobby_id, "host", SteamUser.GetSteamID().ToString() );

            cServer.start();
        }

        private static void onJoinRequested( GameLobbyJoinRequested_t _data ) => join( _data.m_steamIDLobby );

        private static void onEntered( LobbyEnter_t _data )
        {
            m_lobby_id = new CSteamID( _data.m_ulSteamIDLobby );
            cSession.clear();

            string host = SteamMatchmaking.GetLobbyData( m_lobby_id, "host" );
            if( ulong.TryParse( host, out ulong host_id ) )
            {
                cSession.setHost( new CSteamID( host_id ) );

                if( !cSession.isHost() )
                    cClient.connect( cSession.m_host_steam_id );
            }

            cLogger.logInfo( $"Entered lobby: {m_lobby_id}" );
        }
    }
}