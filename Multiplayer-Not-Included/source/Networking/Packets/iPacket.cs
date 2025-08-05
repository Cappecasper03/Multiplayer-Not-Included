using System.IO;

namespace MultiplayerNotIncluded.Networking.Packets
{
    public enum ePacketType
    {
        kSaveFileRequest,
        kSaveFileChunk,

        kPlayerReady,
        kPlayerCursor,

        kDigTool,
        kCancelTool,
        kAttackTool,
        kCaptureTool,
        kClearTool,
        kDeconstructTool,
        kDisconnectTool,
    }

    public interface iIPacket
    {
        ePacketType m_type { get; }

        void serialize( BinaryWriter   _writer );
        void deserialize( BinaryReader _reader );

        void onDispatched();
    }
}