using System;
using System.Collections.Generic;
using MultiplayerNotIncluded.Networking.Packets.Players;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using MultiplayerNotIncluded.Networking.Packets.World;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

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

            s_constructors[ ePacketType.kPlayerReady ]  = () => new cPlayerReadyPacket();
            s_constructors[ ePacketType.kPlayerCursor ] = () => new cPlayerCursorPacket();

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

            s_constructors[ ePacketType.kSpeedChange ] = () => new cSpeedChangePacket();
            s_constructors[ ePacketType.kRedAlert ]    = () => new cRedAlertPacket();

            s_constructors[ ePacketType.kDeconstruct ]     = () => new cDeconstructPacket();
            s_constructors[ ePacketType.kAutoRepair ]      = () => new cAutoRepairPacket();
            s_constructors[ ePacketType.kAutoDisinfect ]   = () => new cAutoDisinfectPacket();
            s_constructors[ ePacketType.kBuildingEnabled ] = () => new cBuildingEnabledPacket();
        }
    }
}