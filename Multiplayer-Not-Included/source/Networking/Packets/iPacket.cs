using System.IO;

namespace MultiplayerNotIncluded.Networking.Packets
{
    public enum ePacketType : byte
    {
        kSaveFileRequest,
        kSaveFileChunk,

        kPlayerReady,
        kPlayerWait,
        kPlayerCursor,
        kPlayerDisconnect,

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
        kTreeFilter,
        kCapacityMeter,
        kSliderSet,
        kDirectionControl,
        kFilter,
        kReconstruct,
        kFabricator,
        kAutoBottle,
        kEntityReceptacle,
        kDropAll,
        kActivationRange,
        kDoorAccess,

        kToggle,

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
        kAssign,
        kRename,

        kPriority,

        kImmigrantScreen,
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