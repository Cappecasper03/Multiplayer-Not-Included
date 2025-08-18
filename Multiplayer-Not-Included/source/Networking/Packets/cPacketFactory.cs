using System;
using System.Collections.Generic;
using MultiplayerNotIncluded.Networking.Packets.Minions;
using MultiplayerNotIncluded.Networking.Packets.Players;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using MultiplayerNotIncluded.Networking.Packets.World;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;
using MultiplayerNotIncluded.Networking.Packets.World.Creatures;
using MultiplayerNotIncluded.Networking.Packets.World.Items;
using MultiplayerNotIncluded.Networking.Packets.World.Plants;

namespace MultiplayerNotIncluded.Networking.Packets
{
    public static class cPacketFactory
    {
        private static readonly Dictionary< ePacketType, Func< iIPacket > > s_constructors = new Dictionary< ePacketType, Func< iIPacket > >();

        public static iIPacket create( ePacketType _type )
        {
            return s_constructors.TryGetValue( _type, out var constructor ) ? constructor() : throw new InvalidOperationException( $"No packet constructor: {_type}" );
        }

        public static void initialize()
        {
            s_constructors[ ePacketType.kSaveFileRequest ] = () => new cSaveFileRequestPacket();
            s_constructors[ ePacketType.kSaveFileChunk ]   = () => new cSaveFileChunkPacket();

            s_constructors[ ePacketType.kPlayerReady ]      = () => new cPlayerReadyPacket();
            s_constructors[ ePacketType.kPlayerWait ]       = () => new cPlayerWaitPacket();
            s_constructors[ ePacketType.kPlayerCursor ]     = () => new cPlayerCursorPacket();
            s_constructors[ ePacketType.kPlayerDisconnect ] = () => new cPlayerDisconnectPacket();

            s_constructors[ ePacketType.kDigTool ]            = () => new cDigToolPacket();
            s_constructors[ ePacketType.kCancelTool ]         = () => new cCancelToolPacket();
            s_constructors[ ePacketType.kAttackTool ]         = () => new cAttackToolPacket();
            s_constructors[ ePacketType.kCaptureTool ]        = () => new cCaptureToolPacket();
            s_constructors[ ePacketType.kClearTool ]          = () => new cClearToolPacket();
            s_constructors[ ePacketType.kDeconstructTool ]    = () => new cDeconstructToolPacket();
            s_constructors[ ePacketType.kDisconnectTool ]     = () => new cDisconnectToolPacket();
            s_constructors[ ePacketType.kDisinfectTool ]      = () => new cDisinfectToolPacket();
            s_constructors[ ePacketType.kEmptyPipeTool ]      = () => new cEmptyPipeToolPacket();
            s_constructors[ ePacketType.kHarvestTool ]        = () => new cHarvestToolPacket();
            s_constructors[ ePacketType.kMopTool ]            = () => new cMopToolPacket();
            s_constructors[ ePacketType.kMoveToLocationTool ] = () => new cMoveToLocationToolPacket();
            s_constructors[ ePacketType.kPrioritizeTool ]     = () => new cPrioritizeToolPacket();
            s_constructors[ ePacketType.kBuildTool ]          = () => new cBuildToolPacket();
            s_constructors[ ePacketType.kCopySettingsTool ]   = () => new cCopySettingsToolPacket();

            s_constructors[ ePacketType.kSpeedChange ] = () => new cSpeedChangePacket();
            s_constructors[ ePacketType.kRedAlert ]    = () => new cRedAlertPacket();

            s_constructors[ ePacketType.kDeconstruct ]      = () => new cDeconstructPacket();
            s_constructors[ ePacketType.kAutoRepair ]       = () => new cAutoRepairPacket();
            s_constructors[ ePacketType.kAutoDisinfect ]    = () => new cAutoDisinfectPacket();
            s_constructors[ ePacketType.kBuildingEnabled ]  = () => new cBuildingEnabledPacket();
            s_constructors[ ePacketType.kCancelBuild ]      = () => new cCancelBuildPacket();
            s_constructors[ ePacketType.kTreeFilter ]       = () => new cTreeFilterPacket();
            s_constructors[ ePacketType.kCapacityMeter ]    = () => new cCapacityMeterPacket();
            s_constructors[ ePacketType.kSliderSet ]        = () => new cSliderSetPacket();
            s_constructors[ ePacketType.kDirectionControl ] = () => new cDirectionControlPacket();
            s_constructors[ ePacketType.kFilter ]           = () => new cFilterPacket();
            s_constructors[ ePacketType.kReconstruct ]      = () => new cReconstructPacket();
            s_constructors[ ePacketType.kFabricator ]       = () => new cFabricatorPacket();

            s_constructors[ ePacketType.kToggle ] = () => new cTogglePacket();

            s_constructors[ ePacketType.kAttackCreature ]  = () => new cAttackCreaturePacket();
            s_constructors[ ePacketType.kCaptureCreature ] = () => new cCaptureCreaturePacket();

            s_constructors[ ePacketType.kClearItem ] = () => new cClearItemPacket();

            s_constructors[ ePacketType.kUproot ]      = () => new cUprootPacket();
            s_constructors[ ePacketType.kAutoHarvest ] = () => new cAutoHarvestPacket();

            s_constructors[ ePacketType.kConsumableInfo ] = () => new cConsumableInfoPacket();
            s_constructors[ ePacketType.kJobPriority ]    = () => new cJobPriorityPacket();
            s_constructors[ ePacketType.kSchedule ]       = () => new cSchedulePacket();
            s_constructors[ ePacketType.kSkills ]         = () => new cSkillsPacket();
            s_constructors[ ePacketType.kResearch ]       = () => new cResearchPacket();
            s_constructors[ ePacketType.kAssign ]         = () => new cAssignPacket();
            s_constructors[ ePacketType.kRename ]         = () => new cRenamePacket();

            s_constructors[ ePacketType.kPriority ] = () => new cPriorityPacket();
        }
    }
}