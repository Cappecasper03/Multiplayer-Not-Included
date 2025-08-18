using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cEntityReceptaclePacket : iIPacket
    {
        private enum eAction
        {
            kCreate,
            kCancel,
            kRemove,
        }

        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private eAction  m_action;
        private int      m_cell;
        private int      m_layer;
        private Tag      m_entity;
        private Tag      m_filter;

        public ePacketType m_type => ePacketType.kEntityReceptacle;

        public static cEntityReceptaclePacket createOrder( int _cell, int _layer, Tag _entity, Tag _filter )
        {
            return new cEntityReceptaclePacket { m_action = eAction.kCreate, m_cell = _cell, m_layer = _layer, m_entity = _entity, m_filter = _filter };
        }

        public static cEntityReceptaclePacket createCancel( int _cell, int _layer )
        {
            return new cEntityReceptaclePacket { m_action = eAction.kCancel, m_cell = _cell, m_layer = _layer, };
        }

        public static cEntityReceptaclePacket createRemove( int _cell, int _layer )
        {
            return new cEntityReceptaclePacket { m_action = eAction.kRemove, m_cell = _cell, m_layer = _layer, };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_cell );
            _writer.Write( m_layer );

            switch( m_action )
            {
                case eAction.kCreate:
                {
                    _writer.Write( m_entity.Name );
                    if( m_filter.Name != null )
                        _writer.Write( m_filter.Name );
                    else
                        _writer.Write( "" );
                    break;
                }
                case eAction.kCancel: break;
                case eAction.kRemove: break;
            }
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_action   = ( eAction )_reader.ReadInt32();
            m_cell     = _reader.ReadInt32();
            m_layer    = _reader.ReadInt32();

            switch( m_action )
            {
                case eAction.kCreate:
                {
                    m_entity = new Tag( _reader.ReadString() );
                    m_filter = new Tag( _reader.ReadString() );
                    break;
                }
                case eAction.kCancel: break;
                case eAction.kRemove: break;
            }
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            if( game_object == null )
                return;

            SingleEntityReceptacle receptacle = game_object.GetComponent< SingleEntityReceptacle >();
            if( receptacle == null )
                return;

            cSingleEntityReceptaclePatch.s_skip_send = true;
            switch( m_action )
            {
                case eAction.kCreate: receptacle.CreateOrder( m_entity, m_filter ); break;
                case eAction.kCancel: receptacle.CancelActiveRequest(); break;
                case eAction.kRemove:
                {
                    cPlantablePlotPatch.s_skip_send = true;
                    receptacle.OrderRemoveOccupant();
                    cPlantablePlotPatch.s_skip_send = false;
                    break;
                }
            }

            cSingleEntityReceptaclePatch.s_skip_send = false;

            ReceptacleSideScreen screen = Object.FindObjectOfType< ReceptacleSideScreen >();
            if( screen != null )
                Traverse.Create( screen ).Method( "UpdateState", new[] { typeof( object ) } )?.GetValue( ( object )null );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kCreate: cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {( Grid.SceneLayer )m_layer}, {m_entity.Name}, {m_filter.Name}" ); break;
                case eAction.kCancel:
                case eAction.kRemove: cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {( Grid.SceneLayer )m_layer}" ); break;
            }
        }
    }
}