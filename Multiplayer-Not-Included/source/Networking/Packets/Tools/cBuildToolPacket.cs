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

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cBuildToolPacket : iIPacket
    {
        private enum eAction
        {
            kBuilding,
            kUtility,
        }

        private CSteamID                   m_steam_id = cSession.m_local_steam_id;
        private eAction                    m_action;
        private string                     m_prefab_id;
        private int                        m_cell;
        private string                     m_facade_id;
        private Orientation                m_orientation;
        private List< string >             m_selected_elements = new List< string >();
        private List< Tuple< int, bool > > m_path              = new List< Tuple< int, bool > >();

        public ePacketType m_type => ePacketType.kBuildTool;

        public static cBuildToolPacket createBuilding( string _prefab_id, int _cell, string _facade_id, Orientation _orientation, IList< Tag > _selected_elements )
        {
            return new cBuildToolPacket
            {
                m_action            = eAction.kBuilding,
                m_prefab_id         = _prefab_id,
                m_cell              = _cell,
                m_facade_id         = _facade_id,
                m_orientation       = _orientation,
                m_selected_elements = _selected_elements.Select( _e => _e.ToString() ).ToList()
            };
        }

        public static cBuildToolPacket createUtility( string _prefab_id, List< Tuple< int, bool > > _path )
        {
            return new cBuildToolPacket
            {
                m_action    = eAction.kUtility,
                m_prefab_id = _prefab_id,
                m_path      = _path
            };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_prefab_id );

            switch( m_action )
            {
                case eAction.kBuilding:
                {
                    _writer.Write( m_cell );
                    _writer.Write( m_facade_id );
                    _writer.Write( ( int )m_orientation );

                    _writer.Write( m_selected_elements.Count );
                    foreach( string element_tag in m_selected_elements )
                        _writer.Write( element_tag );
                    break;
                }
                case eAction.kUtility:
                {
                    _writer.Write( m_path.Count );
                    foreach( Tuple< int, bool > path in m_path )
                    {
                        _writer.Write( path.first );
                        _writer.Write( path.second );
                    }

                    break;
                }
            }
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id  = new CSteamID( _reader.ReadUInt64() );
            m_action    = ( eAction )_reader.ReadInt32();
            m_prefab_id = _reader.ReadString();

            switch( m_action )
            {
                case eAction.kBuilding:
                {
                    m_cell        = _reader.ReadInt32();
                    m_facade_id   = _reader.ReadString();
                    m_orientation = ( Orientation )_reader.ReadInt32();

                    int element_count = _reader.ReadInt32();
                    for( int i = 0; i < element_count; i++ )
                        m_selected_elements.Add( _reader.ReadString() );
                    break;
                }
                case eAction.kUtility:
                {
                    int count = _reader.ReadInt32();
                    for( int i = 0; i < count; i++ )
                        m_path.Add( new Tuple< int, bool >( _reader.ReadInt32(), _reader.ReadBoolean() ) );

                    break;
                }
            }
        }

        public void onReceived()
        {
            BuildingDef building_def = Assets.GetBuildingDef( m_prefab_id );
            if( building_def == null )
                return;

            switch( m_action )
            {
                case eAction.kBuilding:
                {
                    List< Tag > selected_elements = m_selected_elements.Select( _e => new Tag( _e ) ).ToList();
                    Vector3     position          = Grid.CellToPosCBC( m_cell, Grid.SceneLayer.Building );

                    cBuildToolPatch.s_skip_sending = true;
                    string facade = !string.IsNullOrEmpty( m_facade_id ) ? m_facade_id : "DEFAULT_FACADE";
                    building_def.TryPlace( BuildTool.Instance.visualizer, position, m_orientation, selected_elements, facade );
                    cBuildToolPatch.s_skip_sending = false;
                    break;
                }
                case eAction.kUtility:
                {
                    cLogger.logWarning( $"{m_prefab_id}" );
                    BaseUtilityBuildTool instance = null;
                    switch( m_prefab_id )
                    {
                        case "Wire": instance = WireBuildTool.Instance; break;
                        case "LiquidConduit":
                        case "GasConduit":
                        {
                            instance = UtilityBuildTool.Instance;
                            if( instance != null )
                                Traverse.Create( instance ).Field( "def" ).SetValue( building_def );
                            break;
                        }
                    }

                    if( instance == null )
                        return;

                    Type path_node_type = typeof( BaseUtilityBuildTool ).GetNestedType( "PathNode", BindingFlags.NonPublic );
                    if( path_node_type == null )
                        return;

                    IList instance_path = Traverse.Create( instance ).Field( "path" ).GetValue< IList >();
                    if( instance_path == null )
                        return;

                    object path_object = Activator.CreateInstance( path_node_type );
                    instance_path.Clear();
                    foreach( Tuple< int, bool > path in m_path )
                    {
                        path_node_type.GetField( "cell" ).SetValue( path_object, path.first );
                        path_node_type.GetField( "valid" ).SetValue( path_object, path.second );
                        instance_path.Add( path_object );
                    }

                    try
                    {
                        cBaseUtilityBuildToolPatch.s_skip_sending = true;
                        Traverse.Create( instance ).Method( "BuildPath" ).GetValue();
                        cBaseUtilityBuildToolPatch.s_skip_sending = false;
                    }
                    catch( Exception )
                    {
                        cUtils.delayAction( 2000, () => cUtils.initializeUtility( "Power",       "Wire" ) );
                        cUtils.delayAction( 2250, () => cUtils.initializeUtility( "Plumbing",    "LiquidConduit" ) );
                        cUtils.delayAction( 2500, () => cUtils.initializeUtility( "Ventilation", "GasConduit" ) );
                        cUtils.delayAction( 3000, () =>
                        {
                            cBaseUtilityBuildToolPatch.s_skip_sending = true;
                            Traverse.Create( instance ).Method( "BuildPath" ).GetValue();
                            cBaseUtilityBuildToolPatch.s_skip_sending = false;
                        } );
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