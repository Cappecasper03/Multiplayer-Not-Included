using System.IO;

namespace MultiplayerNotIncluded.Networking.Packets
{
    public static class PacketHandler
    {
        public static bool ReadyToProcess = true;

        public static void HandleIncoming( byte[] data )
        {
            if( !ReadyToProcess )
                return;

            MemoryStream memory = new MemoryStream( data );
            BinaryReader reader = new BinaryReader( memory );

            PacketType type   = ( PacketType )reader.ReadByte();
            IPacket    packet = PacketConstructorRegistry.Create( type );

            packet.Deserialize( reader );
            packet.OnDispatched();
        }
    }
}