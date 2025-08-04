using System;
using System.Collections.Generic;
using MultiplayerNotIncluded.Menus;
using MultiplayerNotIncluded.Networking.Packets.World;

namespace MultiplayerNotIncluded.Saves
{
    public static class SaveChunkAssembler
    {
        private class InProgressSave
        {
            public byte[] Data;
            public int    ReceivedBytes;
        }

        private static readonly Dictionary< string, InProgressSave > InProgress = new Dictionary< string, InProgressSave >();

        public static void ReceiveChunk( SaveFileChunkPacket chunk )
        {
            InProgressSave save;
            if( !InProgress.TryGetValue( chunk.FileName, out save ) )
            {
                save = new InProgressSave
                {
                    Data          = new byte[chunk.TotalSize],
                    ReceivedBytes = 0,
                };
                InProgress[ chunk.FileName ] = save;
            }

            Buffer.BlockCopy( chunk.Data, 0, save.Data, chunk.Offset, chunk.Data.Length );
            save.ReceivedBytes += chunk.Data.Length;

            DebugTools.Logger.LogInfo( $"Received {chunk.Data.Length} bytes from '{chunk.FileName}'" );
            MultiplayerLoadingOverlay.Show( $"Synchronizing world: {save.ReceivedBytes * 100 / chunk.TotalSize}%" );

            if( save.ReceivedBytes < chunk.TotalSize )
                return;

            DebugTools.Logger.LogInfo( $"Completed receive of '{chunk.FileName}'" );
            InProgress.Remove( chunk.FileName );

            SaveHelper.LoadWorldSave( chunk.FileName, save.Data );
        }
    }
}