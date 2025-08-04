using System;
using System.Runtime.InteropServices;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets
{
    public static class PacketSender
    {
        private static byte[] SerializePacket( IPacket packet )
        {
            var memory = new System.IO.MemoryStream();
            var writer = new System.IO.BinaryWriter( memory );

            writer.Write( ( byte )packet.Type );
            packet.Serialize( writer );

            return memory.ToArray();
        }

        public static EResult SendToConnection( HSteamNetConnection connection, IPacket packet, SteamNetworkingSend sendType = SteamNetworkingSend.ReliableNoNagle )
        {
            byte[] data = SerializePacket( packet );

            IntPtr dataPtr = Marshal.AllocHGlobal( data.Length );
            Marshal.Copy( data, 0, dataPtr, data.Length );

            EResult result = SteamNetworkingSockets.SendMessageToConnection( connection, dataPtr, ( uint )data.Length, ( int )sendType, out long bytesSent );

            Marshal.FreeHGlobal( dataPtr );
            return result;
        }

        public static EResult SendToPlayer( CSteamID steamID, IPacket packet, SteamNetworkingSend sendType = SteamNetworkingSend.ReliableNoNagle )
        {
            MultiplayerPlayer player;
            if( MultiplayerSession.ConnectedPlayers.TryGetValue( steamID, out player ) && player.Connection != null )
                return SendToConnection( player.Connection.Value, packet, sendType );

            return EResult.k_EResultFail;
        }

        public static EResult SendToHost( IPacket packet, SteamNetworkingSend sendType = SteamNetworkingSend.ReliableNoNagle )
        {
            return MultiplayerSession.HostSteamID.IsValid() ? SendToPlayer( MultiplayerSession.HostSteamID, packet, sendType ) : EResult.k_EResultFail;
        }

        public static EResult SendToAll( IPacket packet, SteamNetworkingSend sendType = SteamNetworkingSend.ReliableNoNagle )
        {
            EResult result = EResult.k_EResultOK;

            foreach( MultiplayerPlayer player in MultiplayerSession.ConnectedPlayers.Values )
            {
                if( !player.IsConnected || player.IsLocal )
                    continue;

                if( SendToConnection( player.Connection.Value, packet, sendType ) != EResult.k_EResultOK )
                    result = EResult.k_EResultFail;
            }

            return result;
        }
    }
}