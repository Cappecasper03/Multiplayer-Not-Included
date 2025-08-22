using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Menus;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Players;
using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class cServer
    {
        public enum eServerState
        {
            kError = -1,
            kStopped,
            kPreparing,
            kStarting,
            kStarted,
        }

        public static eServerState m_state { get; private set; } = eServerState.kStopped;

        private static HSteamListenSocket m_socket     { get; set; }
        private static HSteamNetPollGroup m_poll_group { get; set; }

        private static Callback< SteamNetConnectionStatusChangedCallback_t > s_connection_status_changed_callback;

        public static void start()
        {
            if( m_state > 0 )
                return;

            m_state = eServerState.kPreparing;

            if( !SteamManager.Initialized )
            {
                m_state = eServerState.kError;
                cLogger.logError( "Steam Manager not initialized" );
                return;
            }

            m_state = eServerState.kStarting;

            m_socket = SteamNetworkingSockets.CreateListenSocketP2P( 0, 0, null );
            if( m_socket.m_HSteamListenSocket == 0 )
            {
                m_state = eServerState.kError;
                cLogger.logError( "Failed to create listen socket" );
                return;
            }

            m_poll_group = SteamNetworkingSockets.CreatePollGroup();
            if( m_poll_group.m_HSteamNetPollGroup == 0 )
            {
                m_state = eServerState.kError;
                cLogger.logError( "Failed to create PollGroup" );
                SteamNetworkingSockets.CloseListenSocket( m_socket );
                return;
            }

            if( s_connection_status_changed_callback == null )
                s_connection_status_changed_callback = Callback< SteamNetConnectionStatusChangedCallback_t >.Create( OnConnectionStatusChanged );

            m_state = eServerState.kStarted;
            cLogger.logInfo( "Server started" );
        }

        public static void stop()
        {
            if( m_state <= 0 )
                return;

            cSession.clear();

            s_connection_status_changed_callback?.Unregister();
            s_connection_status_changed_callback = null;

            if( m_poll_group.m_HSteamNetPollGroup != 0 )
                SteamNetworkingSockets.DestroyPollGroup( m_poll_group );

            if( m_socket.m_HSteamListenSocket != 0 )
                SteamNetworkingSockets.CloseListenSocket( m_socket );

            m_state = eServerState.kStopped;
            cLogger.logInfo( "Server stopped" );
        }

        public static void update()
        {
            if( m_state != eServerState.kStarted )
                return;

            SteamAPI.RunCallbacks();
            SteamNetworkingSockets.RunCallbacks();

            const int max_messages = 128;
            IntPtr[]  messages     = new IntPtr[ max_messages ];
            int       count        = SteamNetworkingSockets.ReceiveMessagesOnPollGroup( m_poll_group, messages, max_messages );

            for( int i = 0; i < count; i++ )
            {
                SteamNetworkingMessage_t message = Marshal.PtrToStructure< SteamNetworkingMessage_t >( messages[ i ] );
                byte[]                   data    = new byte[ message.m_cbSize ];
                Marshal.Copy( message.m_pData, data, 0, message.m_cbSize );

                cPacketHandler.handleIncoming( data );
                SteamNetworkingMessage_t.Release( messages[ i ] );
            }
        }

        public static void setWaitingForPlayers()
        {
            if( !SpeedControlScreen.Instance.IsPaused )
                SpeedControlScreen.Instance.Pause( false );

            int              ready_count   = 0;
            string           waiting_for   = "";
            List< CSteamID > ready_players = new List< CSteamID >();
            foreach( cPlayer player in cSession.s_connected_players.Values )
            {
                if( player.m_ready )
                {
                    waiting_for += $"{player.m_steam_name}: Ready\n";
                    ready_count++;
                    ready_players.Add( player.m_steam_id );
                }
                else
                    waiting_for = waiting_for.Insert( 0, $"{player.m_steam_name}: Not Ready\n" );
            }

            waiting_for = $"Waiting for players...({ready_count}/{cSession.s_connected_players.Count})\n{waiting_for}";
            cMultiplayerLoadingOverlay.show( waiting_for );

            cPlayerWaitPacket packet = new cPlayerWaitPacket( waiting_for );
            foreach( CSteamID steam_id in ready_players )
                cPacketSender.sendToPlayer( steam_id, packet );
        }

        private static void tryAcceptConnection( HSteamNetConnection _connection, CSteamID _client_id )
        {
            EResult result = SteamNetworkingSockets.AcceptConnection( _connection );
            if( result == EResult.k_EResultOK )
            {
                SteamNetworkingSockets.SetConnectionPollGroup( _connection, m_poll_group );
                cLogger.logInfo( $"Accepted connection from {_client_id}" );
            }
            else
                rejectConnection( _connection, _client_id, $"Accept failed ({result})" );
        }

        private static void rejectConnection( HSteamNetConnection _connection, CSteamID _client_id, string _reason )
        {
            SteamNetworkingSockets.CloseConnection( _connection, 0, _reason, false );
            cLogger.logError( $"Rejecting connection from {_client_id}: {_reason}" );
        }

        private static void OnClientConnected( HSteamNetConnection _connection, CSteamID _client_id )
        {
            cSession.findOrAddPlayer( _client_id, _connection );
            cLogger.logInfo( $"Connected to {_client_id} on {_connection}" );
        }

        private static void OnClientDisconnected( HSteamNetConnection _connection, CSteamID _client_id )
        {
            SteamNetworkingSockets.CloseConnection( _connection, 0, null, false );
            cSession.removePlayer( _client_id );
            cPacketSender.sendToAllExcluding( new cPlayerDisconnectPacket(), new List< CSteamID > { _client_id } );
            cLogger.logInfo( $"Disconnected from {_client_id}" );
        }

        private static void OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t _data )
        {
            HSteamNetConnection             connection = _data.m_hConn;
            CSteamID                        client_id  = _data.m_info.m_identityRemote.GetSteamID();
            ESteamNetworkingConnectionState state      = _data.m_info.m_eState;

            cLogger.logInfo( $"{client_id} connection status changed: {state}" );

            switch( state )
            {
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                    tryAcceptConnection( connection, client_id );
                    break;
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                    OnClientConnected( connection, client_id );
                    break;
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
                    OnClientDisconnected( connection, client_id );
                    break;
                default:
                    cLogger.logWarning( $"Connection state not managed: {state}" );
                    break;
            }
        }
    }
}