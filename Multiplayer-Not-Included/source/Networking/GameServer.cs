using System;
using System.Linq;
using Steamworks;

namespace MultiplayerNotIncluded.Networking
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

        public static ServerState State { get; private set; } = ServerState.Stopped;

        public static void Start()
        {
            if( State > 0 )
                return;

            State = ServerState.Preparing;

            if( !SteamManager.Initialized )
            {
                State = ServerState.Error;
                DebugTools.Logger.LogError( "Steam Manager not initialized" );
                return;
            }

            State = ServerState.Starting;

            Socket = SteamNetworkingSockets.CreateListenSocketP2P( 0, 0, null );
            if( Socket.m_HSteamListenSocket == 0 )
            {
                State = ServerState.Error;
                DebugTools.Logger.LogError( "Failed to create listen socket" );
                return;
            }

            PollGroup = SteamNetworkingSockets.CreatePollGroup();
            if( PollGroup.m_HSteamNetPollGroup == 0 )
            {
                State = ServerState.Error;
                DebugTools.Logger.LogError( "Failed to create PollGroup" );
                SteamNetworkingSockets.CloseListenSocket( Socket );
                return;
            }

            _connectionStatusChangedCallback =
                Callback< SteamNetConnectionStatusChangedCallback_t >.Create( OnConnectionStatusChanged );

            State                        = ServerState.Started;
            MultiplayerSession.InSession = true;
            DebugTools.Logger.LogInfo( "Server started" );
        }

        public static void Stop()
        {
            if( State <= 0 )
                return;

            foreach( MultiplayerPlayer player in MultiplayerSession.ConnectedPlayers.Values )
            {
                if( player.Connection.HasValue )
                    SteamNetworkingSockets.CloseConnection( player.Connection.Value, 0, "Server Stopping", false );
                player.Connection = null;
            }

            if( PollGroup.m_HSteamNetPollGroup != 0 )
                SteamNetworkingSockets.DestroyPollGroup( PollGroup );

            if( Socket.m_HSteamListenSocket != 0 )
                SteamNetworkingSockets.CloseListenSocket( Socket );

            State                        = ServerState.Stopped;
            MultiplayerSession.InSession = false;
            DebugTools.Logger.LogInfo( "Server stopped" );
        }

        private static void TryAcceptConnection( HSteamNetConnection connection, CSteamID clientId )
        {
            EResult result = SteamNetworkingSockets.AcceptConnection( connection );
            if( result == EResult.k_EResultOK )
            {
                SteamNetworkingSockets.SetConnectionPollGroup( connection, PollGroup );
                DebugTools.Logger.LogInfo( $"Accepted connection from {clientId}" );
            }
            else
                RejectConnection( connection, clientId, $"Accept failed ({result})" );
        }

        private static void RejectConnection( HSteamNetConnection connection, CSteamID clientId, string reason )
        {
            DebugTools.Logger.LogError( $"Rejecting connection from {clientId}: {reason}" );
            SteamNetworkingSockets.CloseConnection( connection, 0, reason, false );
        }

        private static void OnClientConnected( HSteamNetConnection connection, CSteamID clientId )
        {
            MultiplayerPlayer player;
            if( !MultiplayerSession.ConnectedPlayers.TryGetValue( clientId, out player ) )
            {
                player                                          = new MultiplayerPlayer( clientId );
                MultiplayerSession.ConnectedPlayers[ clientId ] = player;
            }

            DebugTools.Logger.LogInfo( $"Connected to {clientId}" );
        }

        private static void OnClientDisconnected( HSteamNetConnection connection, CSteamID clientId )
        {
            SteamNetworkingSockets.CloseConnection( connection, 0, null, false );

            MultiplayerPlayer player;
            if( MultiplayerSession.ConnectedPlayers.TryGetValue( clientId, out player ) )
            {
                player.Connection = null;
                MultiplayerSession.ConnectedPlayers.Remove( clientId );
            }

            DebugTools.Logger.LogInfo( $"Disconnected from {clientId}" );
        }

        private static void OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t data )
        {
            HSteamNetConnection             connection = data.m_hConn;
            CSteamID                        clientId   = data.m_info.m_identityRemote.GetSteamID();
            ESteamNetworkingConnectionState state      = data.m_info.m_eState;

            DebugTools.Logger.LogInfo( $"{clientId} connection status changed: {state}" );

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
                    DebugTools.Logger.LogWarning( $"Connection state not managed: {state}" );
                    break;
            }
        }
    }
}