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
        kDisinfectTool,
        kEmptyPipeTool,
        kHarvestTool,
        kMopTool,
        kMoveToLocationTool,
        kPrioritizeTool,
        kBuildTool,

        kSpeedChange,
        kRedAlert,

        kDeconstruct,
        kAutoRepair,
        kAutoDisinfect,
        kBuildingEnabled,

        kAttackCreature,
        kCaptureCreature,

        kClearItem,

        kUproot,
        kAutoHarvest,

        kConsumableInfo,
        kJobPriority,
        kSchedule,
        kSkills,
    }

    public interface iIPacket
    {
        ePacketType m_type { get; }

        void serialize( BinaryWriter   _writer );
        void deserialize( BinaryReader _reader );

        void onReceived();

        void log( string _message );
    }
}