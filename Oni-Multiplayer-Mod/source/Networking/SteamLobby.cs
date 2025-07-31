using System.Collections.Generic;
using Steamworks;

namespace OniMultiplayerMod.Networking
{
    public static class SteamLobby
    {
        private static Callback< LobbyCreated_t >           _onCreated;
        private static Callback< GameLobbyJoinRequested_t > _onJoinRequested;
        private static Callback< LobbyEnter_t >             _onEntered;

        private static CSteamID CurrentLobbyID { get; set; } = CSteamID.Nil;
        private static bool     InLobby        => CurrentLobbyID.IsValid();

        private static int MaxLobbySize { get; set; } = 4;

        public static void Initialize()
        {
            if( !SteamManager.Initialized )
            {
                Debug.LogError( "[SteamLobby] Steam Manager is not initialized" );
                return;
            }

            _onCreated       = Callback< LobbyCreated_t >.Create( OnCreated );
            _onJoinRequested = Callback< GameLobbyJoinRequested_t >.Create( OnJoinRequested );
            _onEntered       = Callback< LobbyEnter_t >.Create( OnEntered );

            Debug.Log( "[SteamLobby] Callbacks registered" );
        }

        public static void Create( ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic )
        {
            if( !SteamManager.Initialized )
                return;

            SteamMatchmaking.CreateLobby( lobbyType, MaxLobbySize );
        }

        public static void Join( CSteamID lobbyId )
        {
            if( !SteamManager.Initialized )
                return;

            if( InLobby )
            {
                Debug.Log( "[SteamLobby] Already in lobby, leaving current lobby" );
                Leave();
            }

            SteamMatchmaking.JoinLobby( lobbyId );
        }

        public static void Leave()
        {
            if( !SteamManager.Initialized )
                return;

            if( !InLobby )
                return;

            SteamMatchmaking.LeaveLobby( CurrentLobbyID );
            Debug.Log( $"[SteamLobby] Left lobby: {CurrentLobbyID}" );
            CurrentLobbyID = CSteamID.Nil;
        }

        private static List< CSteamID > GetMembers()
        {
            List< CSteamID > members = new List< CSteamID >();

            if( !InLobby )
                return members;

            int count = SteamMatchmaking.GetNumLobbyMembers( CurrentLobbyID );
            for( int i = 0; i < count; i++ )
                members.Add( SteamMatchmaking.GetLobbyMemberByIndex( CurrentLobbyID, i ) );

            return members;
        }

        private static void OnCreated( LobbyCreated_t data )
        {
            if( data.m_eResult != EResult.k_EResultOK )
            {
                Debug.LogError( "[SteamLobby] Failed to create lobby" );
                return;
            }

            CurrentLobbyID = new CSteamID( data.m_ulSteamIDLobby );
            Debug.Log( $"[SteamLobby] Lobby created: {CurrentLobbyID}" );

            SteamMatchmaking.SetLobbyData( CurrentLobbyID, "name", $"{SteamFriends.GetPersonaName()}'s Lobby" );
            SteamMatchmaking.SetLobbyData( CurrentLobbyID, "host", SteamUser.GetSteamID().ToString() );
        }

        private static void OnJoinRequested( GameLobbyJoinRequested_t data )
        {
            Debug.Log( $"[SteamLobby] Joining lobby invited by {data.m_steamIDFriend}" );
        }

        private static void OnEntered( LobbyEnter_t data )
        {
            CurrentLobbyID = new CSteamID( data.m_ulSteamIDLobby );
            Debug.Log( $"[SteamLobby] Entered lobby: {CurrentLobbyID}" );
        }
    }
}