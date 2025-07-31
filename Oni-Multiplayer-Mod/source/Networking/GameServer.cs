using System;
using Steamworks;

namespace OniMultiplayerMod.Networking
{
    public static class GameServer
    {
        public enum ServerState
        {
            Error = -1,
            Stopped,
            Preparing,
            Starting,
            Started,
        }

        private static HSteamListenSocket                                    Socket    { get; set; }
        private static HSteamNetPollGroup                                    PollGroup { get; set; }
        private static Callback< SteamNetConnectionStatusChangedCallback_t > _connectionStatusChangedCallback;

        private static ServerState State { get; set; } = ServerState.Stopped;

        public static void Start()
        {
            State = ServerState.Preparing;

            if( !SteamManager.Initialized )
            {
                State = ServerState.Error;
                Debug.LogError( "[GameServer] Steam Manager not initialized! Cannot start server" );
                return;
            }

            State = ServerState.Starting;

            Socket = SteamNetworkingSockets.CreateListenSocketP2P( 0, 0, null );
            if( Socket.m_HSteamListenSocket == 0 )
            {
                State = ServerState.Error;
                Debug.LogError( "[GameServer] Failed to create listen socket" );
                return;
            }

            PollGroup = SteamNetworkingSockets.CreatePollGroup();
            if( PollGroup.m_HSteamNetPollGroup == 0 )
            {
                State = ServerState.Error;
                Debug.LogError( "[GameServer] Failed to create PollGroup" );
                SteamNetworkingSockets.CloseListenSocket( Socket );
                return;
            }

            _connectionStatusChangedCallback =
                Callback< SteamNetConnectionStatusChangedCallback_t >.Create( OnConnectionStatusChanged );

            Debug.Log( "[GameServer] Server started" );
            State = ServerState.Started;
        }

        public static void Stop()
        {
            State = ServerState.Stopped;

            if( PollGroup.m_HSteamNetPollGroup != 0 )
                SteamNetworkingSockets.DestroyPollGroup( PollGroup );

            if( Socket.m_HSteamListenSocket != 0 )
                SteamNetworkingSockets.CloseListenSocket( Socket );

            Debug.Log( "[GameServer] Server stopped" );
        }

        private static void TryAcceptConnection( HSteamNetConnection connection, CSteamID clientId )
        {
            var result = SteamNetworkingSockets.AcceptConnection( connection );
            if( result == EResult.k_EResultOK )
            {
                SteamNetworkingSockets.SetConnectionPollGroup( connection, PollGroup );
                Debug.Log( $"[GameServer] Accepted connection from {clientId}" );
            }
            else
                RejectConnection( connection, clientId, $"Accept failed ({result})" );
        }

        private static void RejectConnection( HSteamNetConnection connection, CSteamID clientId, string reason )
        {
            Debug.LogError( $"[GameServer] Rejecting connection from {clientId}: {reason}" );
            SteamNetworkingSockets.CloseConnection( connection, 0, reason, false );
        }

        private static void OnClientConnected( HSteamNetConnection connection, CSteamID clientId )
        {
            Debug.Log( $"[GameServer] Connected to {clientId}" );
        }

        private static void OnClientDisconnected( HSteamNetConnection connection, CSteamID clientId )
        {
            SteamNetworkingSockets.CloseConnection( connection, 0, null, false );

            Debug.Log( $"[GameServer] Disconnected from {clientId}" );
        }

        private static void OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t data )
        {
            HSteamNetConnection             connection = data.m_hConn;
            CSteamID                        clientId   = data.m_info.m_identityRemote.GetSteamID();
            ESteamNetworkingConnectionState state      = data.m_info.m_eState;

            Debug.Log( $"[GameServer] {clientId} connection status changed: {state}" );

            switch( state )
            {
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                    TryAcceptConnection( connection, clientId );
                    break;
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                    OnClientConnected( connection, clientId );
                    break;
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
                    OnClientDisconnected( connection, clientId );
                    break;
                default:
                    Debug.LogWarning( $"[GameServer] Connection state not managed: {state}" );
                    break;
            }
        }
    }
}