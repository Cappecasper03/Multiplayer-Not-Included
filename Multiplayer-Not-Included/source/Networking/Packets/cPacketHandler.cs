using System.IO;

namespace MultiplayerNotIncluded.Networking.Packets
{
    public static class cPacketHandler
    {
        public static void handleIncoming( byte[] _data )
        {
            MemoryStream memory = new MemoryStream( _data );
            BinaryReader reader = new BinaryReader( memory );

            ePacketType type   = ( ePacketType )reader.ReadByte();
            iIPacket    packet = cPacketFactory.create( type );

            packet.deserialize( reader );
            packet.onReceived();
            packet.log( "Received" );
        }
    }
}