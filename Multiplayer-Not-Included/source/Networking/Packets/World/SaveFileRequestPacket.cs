using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.Saves;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public class SaveFileRequestPacket : IPacket
    {
        public CSteamID Requester;

        public PacketType Type => PacketType.SaveFileRequest;

        public void Serialize( BinaryWriter writer )
        {
            writer.Write( Requester.m_SteamID );
        }

        public void Deserialize( BinaryReader reader )
        {
            Requester = new CSteamID( reader.ReadUInt64() );
        }

        public void OnDispatched()
        {
            if( !MultiplayerSession.IsHost )
                return;

            string fileName = SaveHelper.WorldName + ".sav";
            byte[] data     = SaveHelper.GetWorldSave();

            const int                    chunkSize = 32 * 1024;
            Queue< SaveFileChunkPacket > packets   = new Queue< SaveFileChunkPacket >();

            for( int offset = 0; offset < data.Length; offset += chunkSize )
            {
                int    size  = Math.Min( chunkSize, data.Length - offset );
                byte[] chunk = new byte[size];
                Buffer.BlockCopy( data, offset, chunk, 0, size );

                SaveFileChunkPacket packet = new SaveFileChunkPacket
                {
                    FileName  = fileName,
                    Offset    = offset,
                    TotalSize = data.Length,
                    Data      = chunk
                };

                packets.Enqueue( packet );
            }

            CoroutineRunner.RunOne( SendChunks( packets, Requester ) );
        }

        private static IEnumerator SendChunks( Queue< SaveFileChunkPacket > packets, CSteamID steamID )
        {
            string fileName    = packets.Peek().FileName;
            int    packetCount = packets.Count;

            while( packets.Count > 0 )
            {
                SaveFileChunkPacket packet = packets.Peek();

                if( PacketSender.SendToPlayer( steamID, packet ) == EResult.k_EResultOK )
                {
                    packets.Dequeue();
                    DebugTools.Logger.LogInfo( $"Sent {packet.Data.Length} bytes from '{packet.FileName}'" );
                }

                yield return null;
            }

            DebugTools.Logger.LogInfo( $"Completed sending of '{fileName}' in {packetCount} chunks to {steamID}" );
        }
    }
}