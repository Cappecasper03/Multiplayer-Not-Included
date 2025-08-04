using System.IO;

namespace MultiplayerNotIncluded.Networking.Packets
{
    public interface IPacket
    {
        PacketType Type { get; }

        void Serialize( BinaryWriter   writer );
        void Deserialize( BinaryReader reader );

        void OnDispatched();
    }
}