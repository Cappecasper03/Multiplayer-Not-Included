using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Saves;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public class cSaveFileRequestPacket : iIPacket
    {
        public CSteamID m_requester;

        public ePacketType m_type => ePacketType.kSaveFileRequest;

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_requester.m_SteamID );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_requester = new CSteamID( _reader.ReadUInt64() );
        }

        public void onDispatched()
        {
            if( !cSession.isHost )
                return;

            string file_name = cSaveHelper.worldName + ".sav";
            byte[] data      = cSaveHelper.getWorldSave();

            const int                     chunk_size = 32 * 1024;
            Queue< cSaveFileChunkPacket > packets    = new Queue< cSaveFileChunkPacket >();

            for( int offset = 0; offset < data.Length; offset += chunk_size )
            {
                int    size  = Math.Min( chunk_size, data.Length - offset );
                byte[] chunk = new byte[ size ];
                Buffer.BlockCopy( data, offset, chunk, 0, size );

                cSaveFileChunkPacket packet = new cSaveFileChunkPacket
                {
                    m_file_name  = file_name,
                    m_offset     = offset,
                    m_total_size = data.Length,
                    m_data       = chunk
                };

                packets.Enqueue( packet );
            }

            CoroutineRunner.RunOne( sendChunks( packets, m_requester ) );
        }

        private static IEnumerator sendChunks( Queue< cSaveFileChunkPacket > _packets, CSteamID _steam_id )
        {
            string file_name    = _packets.Peek().m_file_name;
            int    packet_count = _packets.Count;
            int    total_size   = _packets.Peek().m_total_size;
            int    sent_bytes   = 0;

            while( _packets.Count > 0 )
            {
                cSaveFileChunkPacket packet = _packets.Peek();

                if( cPacketSender.sendToPlayer( _steam_id, packet ) == EResult.k_EResultOK )
                {
                    _packets.Dequeue();
                    sent_bytes += packet.m_data.Length;
                    cLogger.logInfo( $"Sent {packet.m_data.Length} bytes from '{packet.m_file_name}' ({sent_bytes}/{total_size})" );
                }

                yield return null;
            }

            cLogger.logInfo( $"Completed sending of '{file_name}' in {packet_count} chunks to {_steam_id}" );
        }
    }
}