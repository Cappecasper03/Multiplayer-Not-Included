using System;
using System.Collections.Generic;
using MultiplayerNotIncluded.Networking.Packets.Players;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using MultiplayerNotIncluded.Networking.Packets.World;

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

            s_constructors[ ePacketType.kDigTool ]    = () => new cDigToolPacket();
            s_constructors[ ePacketType.kCancelTool ] = () => new cCancelToolPacket();
        }
    }
}