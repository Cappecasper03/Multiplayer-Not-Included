using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.World
{
    public class cRedAlertPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_active;

        public ePacketType m_type => ePacketType.kRedAlert;

        public cRedAlertPacket() {}

        public cRedAlertPacket( bool _active ) => m_active = _active;

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_active );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_active   = _reader.ReadBoolean();
        }

        public void onReceived()
        {
            ClusterManager.Instance.activeWorld.AlertManager.ToggleRedAlert( m_active );

            if( !cSession.isHost() )
                return;

            cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: Active: {m_active}" );
    }
}