using Steamworks;

namespace OniMultiplayerMod.Networking
{
    public class GameClient
    {
        private enum ClientState
        {
            Error = -1,
            Disconnected,
            Connecting,
            Connected,
        }

        private static Callback< SteamNetConnectionStatusChangedCallback_t > _onConnectionStatusChanged;
        private static HSteamNetConnection?                                  Connection { get; set; } = null;
        private static ClientState                                           State      { get; set; } = ClientState.Disconnected;

        public static void Initialize()
        {
            if( _onConnectionStatusChanged != null )
                return;

            _onConnectionStatusChanged = Callback< SteamNetConnectionStatusChangedCallback_t >.Create( OnConnectionStatusChanged );
            Debug.Log( "[GameClient.Initialize] Callback registered" );
        }

        public static void Connect( CSteamID steamId )
        {
            SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
            identity.SetSteamID64( steamId.m_SteamID );

            Connection = SteamNetworkingSockets.ConnectP2P( ref identity, 0, 0, null );
            Debug.Log( $"[SteamNetworkingSockets.Connect] Connect P2P: {Connection.Value.m_HSteamNetConnection}" );
        }

        public static void Disconnect()
        {
            if( !Connection.HasValue )
                return;

            bool result = SteamNetworkingSockets.CloseConnection( Connection.Value, 0, "Client Disconnecting", false );
            Connection = null;
            State      = ClientState.Disconnected;
            Debug.Log( $"[GameClient.Disconnect] Close connection: {result}" );
        }

        private static void OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t data )
        {
            ESteamNetworkingConnectionState state   = data.m_info.m_eState;
            CSteamID                        steamID = data.m_info.m_identityRemote.GetSteamID();

            Debug.Log( $"[GameClient.OnConnectionStatusChanged] {steamID} connection status changed: {state}" );

            switch( state )
            {
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                    OnConnected();
                    break;
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
                    OnDisconnected( "Closed by peer or problem detected locally", steamID );
                    break;
                default:
                    Debug.LogWarning( $"[GameClient.OnConnectionStatusChanged] Connection state not managed: {state}" );
                    break;
            }
        }

        private static void OnConnected()
        {
            State = ClientState.Connected;

            CSteamID hostId = MultiplayerSession.HostSteamID;
            if( !MultiplayerSession.ConnectedPlayers.ContainsKey( hostId ) )
            {
                MultiplayerPlayer player = new MultiplayerPlayer( hostId );
                MultiplayerSession.ConnectedPlayers[ hostId ] = player;
            }

            MultiplayerSession.ConnectedPlayers[ hostId ].Connection = Connection;

            Debug.Log( "[GameClient.OnConnected] Connected to host" );
        }

        private static void OnDisconnected( string reason, CSteamID steamId )
        {
            State = ClientState.Disconnected;
            Debug.Log( $"[GameServer.OnClientDisconnected] Disconnected from {steamId}: {reason}" );
        }
    }
}