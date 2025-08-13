using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cCopySettingsToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private int      m_destination_cell;
        private int      m_source_cell;
        private int      m_layer;

        public ePacketType m_type => ePacketType.kCopySettingsTool;

        public cCopySettingsToolPacket() {}

        public cCopySettingsToolPacket( int _destination_cell, int _source_cell, int _instance_id )
        {
            m_destination_cell = _destination_cell;
            m_source_cell      = _source_cell;
            m_layer            = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_destination_cell );
            _writer.Write( m_source_cell );
            _writer.Write( m_layer );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id         = new CSteamID( _reader.ReadUInt64() );
            m_destination_cell = _reader.ReadInt32();
            m_source_cell      = _reader.ReadInt32();
            m_layer            = _reader.ReadInt32();
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_source_cell, m_layer ];
            if( game_object == null )
                return;

            CopyBuildingSettings.ApplyCopy( m_destination_cell, game_object );
            Game.Instance.userMenu.Refresh( game_object );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_destination_cell}, {m_source_cell}, {( Grid.SceneLayer )m_layer}" );
    }
}