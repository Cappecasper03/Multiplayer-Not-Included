using System;
using System.Runtime.InteropServices;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Menus;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World;
using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class cClient
    {
        private enum eClientState
        {
            kError = -1,
            kDisconnected,
            kConnecting,
            kConnected,
        }

        private static eClientState m_state { get; set; } = eClientState.kDisconnected;

        private static HSteamNetConnection m_connection { get; set; } = HSteamNetConnection.Invalid;

        private static Callback< SteamNetConnectionStatusChangedCallback_t > s_on_connection_status_changed;

        public static void connect( CSteamID _steam_id )
        {
            s_on_connection_status_changed = Callback< SteamNetConnectionStatusChangedCallback_t >.Create( onConnectionStatusChanged );
            m_state                        = eClientState.kConnecting;

            cMultiplayerLoadingOverlay.show( $"Connecting to {SteamFriends.GetFriendPersonaName( _steam_id )}..." );

            SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
            identity.SetSteamID64( _steam_id.m_SteamID );

            m_connection = SteamNetworkingSockets.ConnectP2P( ref identity, 0, 0, null );
            cLogger.logInfo( $"P2P Connection: {m_connection.m_HSteamNetConnection}" );
        }

        public static void disconnect()
        {
            if( m_connection == HSteamNetConnection.Invalid )
                return;

            bool result = SteamNetworkingSockets.CloseConnection( m_connection, 0, "Client Disconnecting", false );
            m_connection = HSteamNetConnection.Invalid;
            m_state      = eClientState.kDisconnected;
            cLogger.logInfo( $"Close connection: {result}" );
        }

        public static void update()
        {
            if( m_state != eClientState.kConnected || m_connection == HSteamNetConnection.Invalid )
                return;

            SteamNetworkingSockets.RunCallbacks();

            const int max_messages = 64;
            IntPtr[]  messages     = new IntPtr[ max_messages ];
            int       count        = SteamNetworkingSockets.ReceiveMessagesOnConnection( m_connection, messages, max_messages );

            for( int i = 0; i < count; i++ )
            {
                SteamNetworkingMessage_t message = Marshal.PtrToStructure< SteamNetworkingMessage_t >( messages[ i ] );
                byte[]                   data    = new byte[ message.m_cbSize ];
                Marshal.Copy( message.m_pData, data, 0, message.m_cbSize );

                cPacketHandler.handleIncoming( data );
                SteamNetworkingMessage_t.Release( messages[ i ] );
            }
        }

        private static void onConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t _data )
        {
            ESteamNetworkingConnectionState state    = _data.m_info.m_eState;
            CSteamID                        steam_id = _data.m_info.m_identityRemote.GetSteamID();

            cLogger.logInfo( $"{steam_id} connection status changed: {state}" );

            switch( state )
            {
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                    onConnected();
                    break;
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
                    onDisconnected( "Closed by peer or problem detected locally", steam_id );
                    break;
                default:
                    cLogger.logWarning( $"Connection state not managed: {state}" );
                    break;
            }
        }

        private static void onConnected()
        {
            m_state = eClientState.kConnected;

            CSteamID host_id = cSession.m_host_steam_id;
            if( !cSession.s_connected_players.ContainsKey( host_id ) )
            {
                cPlayer player = new cPlayer( host_id, m_connection );
                cSession.s_connected_players[ host_id ] = player;
            }

            cLogger.logInfo( "Connected to host" );

            cMultiplayerLoadingOverlay.show( $"Waiting for {SteamFriends.GetFriendPersonaName( cSession.m_host_steam_id )}..." );
            var packet = new cSaveFileRequestPacket( cSession.localSteamID );
            cPacketSender.sendToHost( packet );
        }

        private static void onDisconnected( string _reason, CSteamID _steam_id )
        {
            m_state = eClientState.kDisconnected;
            cLogger.logInfo( $"Disconnected from {_steam_id}: {_reason}" );
        }
    }
}