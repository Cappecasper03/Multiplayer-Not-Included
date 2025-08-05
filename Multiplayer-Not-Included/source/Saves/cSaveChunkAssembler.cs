using System;
using System.Collections.Generic;
using MultiplayerNotIncluded.Menus;
using MultiplayerNotIncluded.Networking.Packets.World;

namespace MultiplayerNotIncluded.Saves
{
    public static class cSaveChunkAssembler
    {
        private struct sInProgressSave
        {
            public byte[] m_data;
            public int    m_received_bytes;
        }

        private static readonly Dictionary< string, sInProgressSave > s_in_progress = new Dictionary< string, sInProgressSave >();

        public static void receiveChunk( cSaveFileChunkPacket _chunk )
        {
            sInProgressSave save;
            if( !s_in_progress.TryGetValue( _chunk.m_file_name, out save ) )
            {
                save = new sInProgressSave
                {
                    m_data           = new byte[_chunk.m_total_size],
                    m_received_bytes = 0,
                };
                s_in_progress[ _chunk.m_file_name ] = save;
            }

            Buffer.BlockCopy( _chunk.m_data, 0, save.m_data, _chunk.m_offset, _chunk.m_data.Length );
            save.m_received_bytes += _chunk.m_data.Length;

            DebugTools.cLogger.logInfo( $"Received {_chunk.m_data.Length} bytes from '{_chunk.m_file_name}'" );
            cMultiplayerLoadingOverlay.show( $"Synchronizing world: {save.m_received_bytes * 100 / _chunk.m_total_size}%" );

            if( save.m_received_bytes < _chunk.m_total_size )
                return;

            DebugTools.cLogger.logInfo( $"Completed receive of '{_chunk.m_file_name}'" );
            s_in_progress.Remove( _chunk.m_file_name );

            cSaveHelper.loadWorldSave( _chunk.m_file_name, save.m_data );
        }
    }
}