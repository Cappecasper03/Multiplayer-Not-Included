using System.Collections.Generic;
using Steamworks;

namespace OniMultiplayerMod.Networking
{
    public static class SteamLobby
    {
        private static Callback< LobbyCreated_t >           _OnCreated;
        private static Callback< GameLobbyJoinRequested_t > _OnJoinRequested;
        private static Callback< LobbyEnter_t >             _OnEntered;

        private static CSteamID CurrentLobbyID { get; set; } = CSteamID.Nil;
        private static bool     InLobby        => CurrentLobbyID.IsValid();

        private static int MaxLobbySize { get; set; } = 4;

        public static void Initialize()
        {
            if( !SteamManager.Initialized )
            {
                Debug.LogError( "[SteamLobby] Steam is not initialized" );
                return;
            }

            _OnCreated       = Callback< LobbyCreated_t >.Create( OnCreated );
            _OnJoinRequested = Callback< GameLobbyJoinRequested_t >.Create( OnJoinRequested );
            _OnEntered       = Callback< LobbyEnter_t >.Create( OnEntered );

            Debug.Log( "[SteamLobby] Callbacks registered" );
        }

        private static void Create( ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic )
        {
            if( !SteamManager.Initialized )
                return;

            SteamMatchmaking.CreateLobby( lobbyType, MaxLobbySize );
            Debug.Log( $"[SteamLobby] Created lobby: {CurrentLobbyID}" );
        }

        private static void Join()
        {
            if( !SteamManager.Initialized )
                return;

            if( InLobby )
            {
                Debug.Log( "[SteamLobby] Already in lobby, leaving current lobby" );
                Leave();
            }

            SteamMatchmaking.JoinLobby( CurrentLobbyID );
            Debug.Log( $"[SteamLobby] Joined lobby: {CurrentLobbyID}" );
        }

        private static void Leave()
        {
            if( !SteamManager.Initialized )
                return;

            if( !InLobby )
                return;

            SteamMatchmaking.LeaveLobby( CurrentLobbyID );
            CurrentLobbyID = CSteamID.Nil;
            Debug.Log( $"[SteamLobby] Left lobby: {CurrentLobbyID}" );
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

        private static void OnCreated( LobbyCreated_t callback )
        {
            if( callback.m_eResult != EResult.k_EResultOK )
            {
                Debug.LogError( "[SteamLobby] Failed to create lobby" );
                return;
            }

            CurrentLobbyID = new CSteamID( callback.m_ulSteamIDLobby );
            Debug.Log( "[SteamLobby] Lobby created" );

            SteamMatchmaking.SetLobbyData( CurrentLobbyID, "name", $"{SteamFriends.GetPersonaName()}'s Lobby" );
            SteamMatchmaking.SetLobbyData( CurrentLobbyID, "host", SteamUser.GetSteamID().ToString() );
        }

        private static void OnJoinRequested( GameLobbyJoinRequested_t callback )
        {
            Debug.Log( $"[SteamLobby] Joining lobby invited by {callback.m_steamIDFriend}" );
        }

        private static void OnEntered( LobbyEnter_t callback )
        {
            CurrentLobbyID = new CSteamID( callback.m_ulSteamIDLobby );
            Debug.Log( $"[SteamLobby] Entered lobby: {CurrentLobbyID}" );
        }
    }
}