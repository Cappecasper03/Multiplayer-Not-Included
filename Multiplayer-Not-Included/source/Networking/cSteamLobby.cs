using System.Collections.Generic;
using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class cSteamLobby
    {
        private static Callback< LobbyCreated_t >           s_on_created;
        private static Callback< GameLobbyJoinRequested_t > s_on_join_requested;
        private static Callback< LobbyEnter_t >             s_on_entered;

        public static CSteamID m_current_lobby_id { get; private set; } = CSteamID.Nil;
        public static bool     inLobby            => m_current_lobby_id.IsValid();

        private static int m_max_lobby_size { get; set; } = 4;

        public static void initialize()
        {
            if( !SteamManager.Initialized )
            {
                DebugTools.cLogger.logError( "Steam Manager is not initialized" );
                return;
            }

            s_on_created        = Callback< LobbyCreated_t >.Create( onCreated );
            s_on_join_requested = Callback< GameLobbyJoinRequested_t >.Create( onJoinRequested );
            s_on_entered        = Callback< LobbyEnter_t >.Create( onEntered );

            DebugTools.cLogger.logInfo( "Callbacks registered" );
        }

        public static void create( ELobbyType _lobby_type = ELobbyType.k_ELobbyTypeFriendsOnly )
        {
            if( !SteamManager.Initialized )
                return;

            if( inLobby )
            {
                DebugTools.cLogger.logInfo( "Already in another lobby, leaving current lobby" );
                leave();
            }

            SteamMatchmaking.CreateLobby( _lobby_type, m_max_lobby_size );
        }

        private static void join( CSteamID _lobby_id )
        {
            if( !SteamManager.Initialized )
                return;

            if( inLobby )
            {
                DebugTools.cLogger.logInfo( "Already in another lobby, leaving current lobby" );
                leave();
            }

            DebugTools.cLogger.logInfo( $"Joining lobby: {_lobby_id}" );
            SteamMatchmaking.JoinLobby( _lobby_id );
        }

        public static void leave()
        {
            if( !SteamManager.Initialized )
                return;

            if( !inLobby )
                return;

            cGameServer.stop();
            cMultiplayerSession.clear();

            SteamMatchmaking.LeaveLobby( m_current_lobby_id );
            DebugTools.cLogger.logInfo( $"Left lobby: {m_current_lobby_id}" );
            m_current_lobby_id = CSteamID.Nil;
        }

        private static void onCreated( LobbyCreated_t _data )
        {
            if( _data.m_eResult != EResult.k_EResultOK )
            {
                DebugTools.cLogger.logError( "Failed to create lobby" );
                return;
            }

            m_current_lobby_id = new CSteamID( _data.m_ulSteamIDLobby );
            DebugTools.cLogger.logInfo( $"Lobby created: {m_current_lobby_id}" );

            SteamMatchmaking.SetLobbyData( m_current_lobby_id, "name", $"{SteamFriends.GetPersonaName()}'s Lobby" );
            SteamMatchmaking.SetLobbyData( m_current_lobby_id, "host", SteamUser.GetSteamID().ToString() );

            cGameServer.start();
        }

        private static void onJoinRequested( GameLobbyJoinRequested_t _data )
        {
            join( _data.m_steamIDLobby );
        }

        private static void onEntered( LobbyEnter_t _data )
        {
            m_current_lobby_id = new CSteamID( _data.m_ulSteamIDLobby );

            cMultiplayerSession.clear();

            string host = SteamMatchmaking.GetLobbyData( m_current_lobby_id, "host" );
            if( ulong.TryParse( host, out ulong host_id ) )
                cMultiplayerSession.setHost( new CSteamID( host_id ) );

            if( !cMultiplayerSession.isHost && cMultiplayerSession.m_host_steam_id.IsValid() )
                cGameClient.connect( cMultiplayerSession.m_host_steam_id );

            DebugTools.cLogger.logInfo( $"Entered lobby: {m_current_lobby_id}" );
        }
    }
}