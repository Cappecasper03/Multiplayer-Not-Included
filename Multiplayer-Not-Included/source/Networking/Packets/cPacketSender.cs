using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets
{
    public static class cPacketSender
    {
        private static byte[] serializePacket( iIPacket _packet )
        {
            var memory = new System.IO.MemoryStream();
            var writer = new System.IO.BinaryWriter( memory );

            writer.Write( ( byte )_packet.m_type );
            _packet.serialize( writer );

            return memory.ToArray();
        }

        private static EResult sendToConnection( HSteamNetConnection _connection, iIPacket _packet, eSteamNetworkingSend _send_type = eSteamNetworkingSend.kReliableNoNagle )
        {
            byte[] data = serializePacket( _packet );

            IntPtr data_ptr = Marshal.AllocHGlobal( data.Length );
            Marshal.Copy( data, 0, data_ptr, data.Length );

            long    bytes_sent;
            EResult result = SteamNetworkingSockets.SendMessageToConnection( _connection, data_ptr, ( uint )data.Length, ( int )_send_type, out bytes_sent );

            _packet.log( $"Send({bytes_sent})" );
            Marshal.FreeHGlobal( data_ptr );
            return result;
        }

        public static EResult sendToPlayer( CSteamID _steam_id, iIPacket _packet, eSteamNetworkingSend _send_type = eSteamNetworkingSend.kReliableNoNagle )
        {
            cPlayer player;
            if( cSession.tryGetPlayer( _steam_id, out player ) )
                return sendToConnection( player.m_connection, _packet, _send_type );

            return EResult.k_EResultFail;
        }

        public static EResult sendToHost( iIPacket _packet, eSteamNetworkingSend _send_type = eSteamNetworkingSend.kReliableNoNagle )
        {
            if( cSession.m_host_steam_id.IsValid() )
                return sendToPlayer( cSession.m_host_steam_id, _packet, _send_type );

            return EResult.k_EResultFail;
        }

        public static EResult sendToAll( iIPacket _packet, eSteamNetworkingSend _send_type = eSteamNetworkingSend.kReliableNoNagle )
        {
            EResult result = EResult.k_EResultOK;

            foreach( cPlayer player in cSession.s_connected_players.Values )
            {
                if( !player.isConnected() || player.isLocal() )
                    continue;

                if( sendToConnection( player.m_connection, _packet, _send_type ) != EResult.k_EResultOK )
                    result = EResult.k_EResultFail;
            }

            return result;
        }

        public static EResult sendToAllExcluding( iIPacket _packet, List< CSteamID > _excluded, eSteamNetworkingSend _send_type = eSteamNetworkingSend.kReliableNoNagle )
        {
            EResult result = EResult.k_EResultOK;

            foreach( cPlayer player in cSession.s_connected_players.Values )
            {
                if( !player.isConnected() || player.isLocal() || _excluded.Contains( player.m_steam_id ) )
                    continue;

                if( sendToConnection( player.m_connection, _packet, _send_type ) != EResult.k_EResultOK )
                    result = EResult.k_EResultFail;
            }

            return result;
        }
    }
}