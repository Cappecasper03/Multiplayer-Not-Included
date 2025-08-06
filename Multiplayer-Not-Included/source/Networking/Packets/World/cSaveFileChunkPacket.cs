using System.IO;
using MultiplayerNotIncluded.Saves;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public class cSaveFileChunkPacket : iIPacket
    {
        public string m_file_name;
        public int    m_offset;
        public int    m_total_size;
        public byte[] m_data;

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

        public void onReceived() => cSaveChunkAssembler.receiveChunk( this );

        public void log( string _message ) {}
    }
}