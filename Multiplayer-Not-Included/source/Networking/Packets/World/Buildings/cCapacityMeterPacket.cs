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
    public class cCapacityMeterPacket : iIPacket
    {
        public enum eComponentType
        {
            kNone,
            kBaggableCritterCapacityTracker,
            kBottler,
            kCargoBayCluster,
            kFuelTank,
            kHEPFuelTank,
            kObjectDispenser,
            kOxidizerTank,
            kRationBox,
            kRefrigerator,
            kStorageLocker,
        }

        private CSteamID       m_steam_id = cSession.m_local_steam_id;
        private eComponentType m_component_type;
        private float          m_value;
        private int            m_cell;
        private int            m_layer;

        public ePacketType m_type => ePacketType.kCapacityMeter;

        public cCapacityMeterPacket() {}

        public cCapacityMeterPacket( eComponentType _type, float _value, int _cell, int _layer )
        {
            m_component_type = _type;
            m_value          = _value;
            m_cell           = _cell;
            m_layer          = _layer;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_component_type );
            _writer.Write( m_value );
            _writer.Write( m_cell );
            _writer.Write( m_layer );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id       = new CSteamID( _reader.ReadUInt64() );
            m_component_type = ( eComponentType )_reader.ReadInt32();
            m_value          = _reader.ReadSingle();
            m_cell           = _reader.ReadInt32();
            m_layer          = _reader.ReadInt32();
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            if( game_object == null )
                return;

            IUserControlledCapacity capacity_control = null;
            switch( m_component_type )
            {
                case eComponentType.kNone:                           return;
                case eComponentType.kBaggableCritterCapacityTracker: capacity_control = game_object.GetComponent< BaggableCritterCapacityTracker >(); break;
                case eComponentType.kBottler:                        capacity_control = game_object.GetComponent< Bottler >(); break;
                case eComponentType.kCargoBayCluster:                capacity_control = game_object.GetComponent< CargoBayCluster >(); break;
                case eComponentType.kFuelTank:                       capacity_control = game_object.GetComponent< FuelTank >(); break;
                case eComponentType.kHEPFuelTank:                    capacity_control = game_object.GetComponent< HEPFuelTank >(); break;
                case eComponentType.kObjectDispenser:                capacity_control = game_object.GetComponent< ObjectDispenser >(); break;
                case eComponentType.kOxidizerTank:                   capacity_control = game_object.GetComponent< OxidizerTank >(); break;
                case eComponentType.kRationBox:                      capacity_control = game_object.GetComponent< RationBox >(); break;
                case eComponentType.kRefrigerator:                   capacity_control = game_object.GetComponent< Refrigerator >(); break;
                case eComponentType.kStorageLocker:                  capacity_control = game_object.GetComponent< StorageLocker >(); break;
            }

            if( capacity_control == null )
                return;

            CapacityControlSideScreen screen = Object.FindObjectOfType< CapacityControlSideScreen >();
            if( screen != null )
            {
                IUserControlledCapacity target = Traverse.Create( screen ).Field( "target" ).GetValue< IUserControlledCapacity >();
                if( capacity_control == target )
                {
                    cCapacityControlSideScreenPatch.s_skip_send = true;
                    Traverse.Create( screen ).Method( "UpdateMaxCapacity", new[] { typeof( float ) } )?.GetValue( m_value );
                    cCapacityControlSideScreenPatch.s_skip_send = false;
                }
                else
                    capacity_control.UserMaxCapacity = m_value;
            }
            else
                capacity_control.UserMaxCapacity = m_value;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_component_type}, {m_value}, {m_cell}, {( Grid.SceneLayer )m_layer}" );
    }
}