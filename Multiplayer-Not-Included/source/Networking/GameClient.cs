using System;
using System.Runtime.InteropServices;
using MultiplayerNotIncluded.Menus;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World;
using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class GameClient
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

        public static void Connect( CSteamID steamId )
        {
            _onConnectionStatusChanged = Callback< SteamNetConnectionStatusChangedCallback_t >.Create( OnConnectionStatusChanged );
            State                      = ClientState.Connecting;

            MultiplayerLoadingOverlay.Show( $"Connecting to {SteamFriends.GetFriendPersonaName( steamId )}" );

            SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
            identity.SetSteamID64( steamId.m_SteamID );

            Connection = SteamNetworkingSockets.ConnectP2P( ref identity, 0, 0, null );
            DebugTools.Logger.LogInfo( $"P2P Connection: {Connection.Value.m_HSteamNetConnection}" );
        }

        public static void Disconnect()
        {
            if( !Connection.HasValue )
                return;

            bool result = SteamNetworkingSockets.CloseConnection( Connection.Value, 0, "Client Disconnecting", false );
            Connection = null;
            State      = ClientState.Disconnected;
            DebugTools.Logger.LogInfo( $"Close connection: {result}" );
        }

        public static void Update()
        {
            if( State != ClientState.Connected || !Connection.HasValue )
                return;

            SteamNetworkingSockets.RunCallbacks();

            const int maxMessages = 64;
            IntPtr[]  messages    = new IntPtr[maxMessages];
            int       count       = SteamNetworkingSockets.ReceiveMessagesOnConnection( Connection.Value, messages, maxMessages );

            for( int i = 0; i < count; i++ )
            {
                SteamNetworkingMessage_t message = Marshal.PtrToStructure< SteamNetworkingMessage_t >( messages[ i ] );
                byte[]                   data    = new byte[message.m_cbSize];
                Marshal.Copy( message.m_pData, data, 0, message.m_cbSize );

                PacketHandler.HandleIncoming( data );
                SteamNetworkingMessage_t.Release( messages[ i ] );
            }
        }

        private static void OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t data )
        {
            ESteamNetworkingConnectionState state   = data.m_info.m_eState;
            CSteamID                        steamID = data.m_info.m_identityRemote.GetSteamID();

            DebugTools.Logger.LogInfo( $"{steamID} connection status changed: {state}" );

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
                    DebugTools.Logger.LogWarning( $"Connection state not managed: {state}" );
                    break;
            }
        }

        private static void OnConnected()
        {
            State                        = ClientState.Connected;
            MultiplayerSession.InSession = true;

            CSteamID hostId = MultiplayerSession.HostSteamID;
            if( !MultiplayerSession.ConnectedPlayers.ContainsKey( hostId ) )
            {
                MultiplayerPlayer player = new MultiplayerPlayer( hostId );
                MultiplayerSession.ConnectedPlayers[ hostId ] = player;
            }

            MultiplayerSession.ConnectedPlayers[ hostId ].Connection = Connection;
            DebugTools.Logger.LogInfo( "Connected to host" );

            var packet = new SaveFileRequestPacket
            {
                Requester = MultiplayerSession.LocalSteamID,
            };
            PacketSender.SendToHost( packet );
        }

        private static void OnDisconnected( string reason, CSteamID steamId )
        {
            State                        = ClientState.Disconnected;
            MultiplayerSession.InSession = false;
            DebugTools.Logger.LogInfo( $"Disconnected from {steamId}: {reason}" );
        }
    }
}