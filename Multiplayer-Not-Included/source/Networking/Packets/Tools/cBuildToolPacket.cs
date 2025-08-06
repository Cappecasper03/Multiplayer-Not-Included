using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cBuildToolPacket : iIPacket
    {
        private CSteamID       m_steam_id = cSession.m_local_steam_id;
        private int            m_cell;
        private string         m_prefab_id;
        private string         m_facade_id;
        private Orientation    m_orientation;
        private List< string > m_selected_elements = new List< string >();

        public ePacketType m_type => ePacketType.kBuildTool;

        public cBuildToolPacket() {}

        public cBuildToolPacket( int _cell, string _prefab_id, string _facade_id, Orientation _orientation, IList< Tag > _selected_elements )
        {
            m_cell              = _cell;
            m_prefab_id         = _prefab_id;
            m_facade_id         = _facade_id;
            m_orientation       = _orientation;
            m_selected_elements = _selected_elements.Select( _e => _e.ToString() ).ToList();
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_cell );
            _writer.Write( m_prefab_id );
            _writer.Write( m_facade_id );
            _writer.Write( ( int )m_orientation );

            _writer.Write( m_selected_elements.Count );
            foreach( string element_tag in m_selected_elements )
                _writer.Write( element_tag );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_cell        = _reader.ReadInt32();
            m_prefab_id   = _reader.ReadString();
            m_facade_id   = _reader.ReadString();
            m_orientation = ( Orientation )_reader.ReadInt32();

            int element_count = _reader.ReadInt32();
            for( int i = 0; i < element_count; i++ )
                m_selected_elements.Add( _reader.ReadString() );
        }

        public void onReceived()
        {
            if( !Grid.IsValidCell( m_cell ) )
                return;

            BuildingDef building_def = Assets.GetBuildingDef( m_prefab_id );
            if( building_def == null )
                return;

            List< Tag > selected_elements = m_selected_elements.Select( _e => new Tag( _e ) ).ToList();
            Vector3     position          = Grid.CellToPosCBC( m_cell, Grid.SceneLayer.Building );

            GameObject visualizer = Util.KInstantiate( building_def.BuildingPreview, position );
            building_def.TryPlace( visualizer, position, m_orientation, selected_elements, !string.IsNullOrEmpty( m_facade_id ) ? m_facade_id : "DEFAULT_FACADE" );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_steam_id} ({m_prefab_id})" );
    }
}