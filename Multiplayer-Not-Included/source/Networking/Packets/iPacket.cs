using System.IO;

namespace MultiplayerNotIncluded.Networking.Packets
{
    public enum ePacketType
    {
        kSaveFileRequest,
        kSaveFileChunk,

        kPlayerReady,
        kPlayerWait,
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
        kCopySettingsTool,

        kSpeedChange,
        kRedAlert,

        kDeconstruct,
        kAutoRepair,
        kAutoDisinfect,
        kBuildingEnabled,
        kCancelBuild,

        kAttackCreature,
        kCaptureCreature,

        kClearItem,

        kUproot,
        kAutoHarvest,

        kConsumableInfo,
        kJobPriority,
        kSchedule,
        kSkills,
        kResearch,

        kPriority,
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