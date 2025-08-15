using System;
using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Menus;
using MultiplayerNotIncluded.Saves;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public class cSaveFileChunkPacket : iIPacket
    {
        private class cInProgressSave
        {
            public byte[] m_data;
            public int    m_received_bytes;
        }

        public string m_file_name;
        public int    m_offset;
        public int    m_total_size;
        public byte[] m_data;

        private static readonly Dictionary< string, cInProgressSave > s_in_progress = new Dictionary< string, cInProgressSave >();

        public ePacketType m_type => ePacketType.kSaveFileChunk;

        public cSaveFileChunkPacket() {}

        public cSaveFileChunkPacket( string _file_name, int _offset, int _total_size, byte[] _data )
        {
            m_file_name  = _file_name;
            m_offset     = _offset;
            m_total_size = _total_size;
            m_data       = _data;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_file_name );
            _writer.Write( m_offset );
            _writer.Write( m_total_size );
            _writer.Write( m_data.Length );
            _writer.Write( m_data );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_file_name  = _reader.ReadString();
            m_offset     = _reader.ReadInt32();
            m_total_size = _reader.ReadInt32();

            int length = _reader.ReadInt32();
            m_data = _reader.ReadBytes( length );
        }

        public void onReceived()
        {
            cInProgressSave save;
            if( !s_in_progress.TryGetValue( m_file_name, out save ) )
            {
                save = new cInProgressSave
                {
                    m_data           = new byte[ m_total_size ],
                    m_received_bytes = 0,
                };
                s_in_progress[ m_file_name ] = save;
            }

            Buffer.BlockCopy( m_data, 0, save.m_data, m_offset, m_data.Length );
            save.m_received_bytes += m_data.Length;

            cLogger.logInfo( $"Received {m_data.Length} bytes from '{m_file_name}' ({save.m_received_bytes}/{m_total_size})" );
            cMultiplayerLoadingOverlay.show( $"Synchronizing world: {save.m_received_bytes * 100 / m_total_size}%" );

            if( save.m_received_bytes < m_total_size )
                return;

            cLogger.logInfo( $"Completed receive of '{m_file_name}'" );
            s_in_progress.Remove( m_file_name );

            cSaveHelper.loadWorldSave( m_file_name, save.m_data );
        }

        public void log( string _message ) {}
    }
}