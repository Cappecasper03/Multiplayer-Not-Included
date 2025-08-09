using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.Tool.Build;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cBuildToolPacket : iIPacket
    {
        private enum eAction
        {
            kBuilding,
            kUtility,
        }

        private CSteamID           m_steam_id = cSession.m_local_steam_id;
        private eAction            m_action;
        private string             m_prefab_id;
        private string             m_facade_id;
        private List< string >     m_selected_elements = new List< string >();
        private int                m_cell;
        private Orientation        m_orientation;
        private List< int >        m_path        = new List< int >();
        List< UtilityConnections > m_connections = new List< UtilityConnections >();

        public ePacketType m_type => ePacketType.kBuildTool;

        public static cBuildToolPacket createBuilding( string _prefab_id, string _facade_id, IList< Tag > _selected_elements, int _cell, Orientation _orientation )
        {
            return new cBuildToolPacket
            {
                m_action            = eAction.kBuilding,
                m_prefab_id         = _prefab_id,
                m_facade_id         = _facade_id,
                m_selected_elements = _selected_elements.Select( _e => _e.ToString() ).ToList(),
                m_cell              = _cell,
                m_orientation       = _orientation,
            };
        }

        public static cBuildToolPacket createUtility( string _prefab_id, string _facade_id, IList< Tag > _selected_elements, List< int > _path, List< UtilityConnections > _connections )
        {
            return new cBuildToolPacket
            {
                m_action            = eAction.kUtility,
                m_prefab_id         = _prefab_id,
                m_facade_id         = _facade_id,
                m_selected_elements = _selected_elements.Select( _e => _e.ToString() ).ToList(),
                m_path              = _path,
                m_connections       = _connections,
            };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_prefab_id );
            _writer.Write( m_facade_id );

            _writer.Write( m_selected_elements.Count );
            foreach( string element_tag in m_selected_elements )
                _writer.Write( element_tag );

            switch( m_action )
            {
                case eAction.kBuilding:
                {
                    _writer.Write( m_cell );
                    _writer.Write( ( int )m_orientation );

                    break;
                }
                case eAction.kUtility:
                {
                    _writer.Write( m_path.Count );
                    foreach( int cell in m_path )
                        _writer.Write( cell );

                    _writer.Write( m_connections.Count );
                    foreach( UtilityConnections connection in m_connections )
                        _writer.Write( ( int )connection );

                    break;
                }
            }
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id  = new CSteamID( _reader.ReadUInt64() );
            m_action    = ( eAction )_reader.ReadInt32();
            m_prefab_id = _reader.ReadString();
            m_facade_id = _reader.ReadString();

            int element_count = _reader.ReadInt32();
            for( int i = 0; i < element_count; i++ )
                m_selected_elements.Add( _reader.ReadString() );

            switch( m_action )
            {
                case eAction.kBuilding:
                {
                    m_cell        = _reader.ReadInt32();
                    m_orientation = ( Orientation )_reader.ReadInt32();

                    break;
                }
                case eAction.kUtility:
                {
                    int cell_count = _reader.ReadInt32();
                    for( int i = 0; i < cell_count; i++ )
                        m_path.Add( _reader.ReadInt32() );

                    int connection_count = _reader.ReadInt32();
                    for( int i = 0; i < connection_count; i++ )
                        m_connections.Add( ( UtilityConnections )_reader.ReadInt32() );

                    break;
                }
            }
        }

        public void onReceived()
        {
            BuildingDef building_def = Assets.GetBuildingDef( m_prefab_id );
            if( building_def == null )
                return;

            List< Tag > selected_elements = m_selected_elements.Select( _e => new Tag( _e ) ).ToList();
            string      facade            = !string.IsNullOrEmpty( m_facade_id ) ? m_facade_id : "DEFAULT_FACADE";

            switch( m_action )
            {
                case eAction.kBuilding:
                {
                    Vector3 position = Grid.CellToPosCBC( m_cell, building_def.SceneLayer );

                    cBuildToolPatch.s_skip_sending = true;
                    building_def.TryPlace( BuildTool.Instance.visualizer, position, m_orientation, selected_elements, facade );
                    cBuildToolPatch.s_skip_sending = false;
                    break;
                }
                case eAction.kUtility:
                {
                    if( m_path.Count == 1 )
                    {
                        Vector3 position = Grid.CellToPosCBC( m_path[ 0 ], building_def.SceneLayer );

                        building_def.TryPlace( null, position, Orientation.Neutral, selected_elements, facade );
                    }
                    else
                    {
                        IUtilityNetworkMgr manager = building_def.BuildingComplete.GetComponent< IHaveUtilityNetworkMgr >().GetNetworkManager();
                        if( manager == null )
                            return;

                        for( int i = 0; i < m_path.Count; i++ )
                        {
                            int                cell       = m_path[ i ];
                            UtilityConnections connection = m_connections[ i ];

                            Vector3 position = Grid.CellToPosCBC( cell, building_def.SceneLayer );

                            GameObject utility = building_def.TryPlace( null, position, Orientation.Neutral, selected_elements, facade );
                            manager.SetConnections( connection, cell, false );

                            KAnimGraphTileVisualizer component = utility.GetComponent< KAnimGraphTileVisualizer >();
                            if( component != null )
                                component.UpdateConnections( component.Connections | manager.GetConnections( cell, false ) );
                        }
                    }

                    break;
                }
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kBuilding: cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {m_prefab_id}, {m_facade_id}, {m_orientation}, {m_selected_elements.Count}" ); break;
                case eAction.kUtility:  cLogger.logInfo( $"{_message}: {m_action}, {m_prefab_id}, {m_path.Count}" ); break;
            }
        }
    }
}