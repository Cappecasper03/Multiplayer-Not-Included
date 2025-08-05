using System.IO;

namespace MultiplayerNotIncluded.Networking.Packets
{
    public static class cPacketHandler
    {
        public static bool s_ready_to_process = true;

        public static void handleIncoming( byte[] _data )
        {
            if( !s_ready_to_process )
                return;

            MemoryStream memory = new MemoryStream( _data );
            BinaryReader reader = new BinaryReader( memory );

            ePacketType type   = ( ePacketType )reader.ReadByte();
            iIPacket    packet = cPacketFactory.create( type );

            packet.deserialize( reader );
            packet.onDispatched();
        }
    }
}