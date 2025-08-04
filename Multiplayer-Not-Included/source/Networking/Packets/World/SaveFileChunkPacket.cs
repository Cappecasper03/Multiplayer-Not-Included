using System.IO;
using MultiplayerNotIncluded.Saves;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public class SaveFileChunkPacket : IPacket
    {
        public string FileName;
        public int    Offset;
        public int    TotalSize;
        public byte[] Data;

        public PacketType Type => PacketType.SaveFileChunk;

        public void Serialize( BinaryWriter writer )
        {
            writer.Write( FileName );
            writer.Write( Offset );
            writer.Write( TotalSize );
            writer.Write( Data.Length );
            writer.Write( Data );
        }

        public void Deserialize( BinaryReader reader )
        {
            FileName  = reader.ReadString();
            Offset    = reader.ReadInt32();
            TotalSize = reader.ReadInt32();
            int length = reader.ReadInt32();
            Data = reader.ReadBytes( length );
        }

        public void OnDispatched()
        {
            SaveChunkAssembler.ReceiveChunk( this );
        }
    }
}