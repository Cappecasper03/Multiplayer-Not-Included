using System;
using System.Collections.Generic;
using MultiplayerNotIncluded.Networking.Packets.World;

namespace MultiplayerNotIncluded.Networking.Packets
{
    public static class PacketConstructorRegistry
    {
        private static readonly Dictionary< PacketType, Func< IPacket > > Constructors = new Dictionary< PacketType, Func< IPacket > >();

        public static IPacket Create( PacketType type )
        {
            return Constructors.TryGetValue( type, out var constructor ) ? constructor() : throw new InvalidOperationException( $"No packet constructor: {type}" );
        }

        public static void Initialize()
        {
            Constructors[ PacketType.SaveFileRequest ] = () => new SaveFileRequestPacket();
            Constructors[ PacketType.SaveFileChunk ]   = () => new SaveFileChunkPacket();
        }
    }
}