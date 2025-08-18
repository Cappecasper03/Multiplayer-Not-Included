using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cActivationRangePacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_active;
        private float    m_value;
        private int      m_cell;
        private int      m_layer;

        public ePacketType m_type => ePacketType.kActivationRange;

        public cActivationRangePacket() {}

        public cActivationRangePacket( bool _active, float _value, int _cell, int _layer )
        {
            m_active = _active;
            m_value  = _value;
            m_cell   = _cell;
            m_layer  = _layer;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_active );
            _writer.Write( m_value );
            _writer.Write( m_cell );
            _writer.Write( m_layer );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_active   = _reader.ReadBoolean();
            m_value    = _reader.ReadSingle();
            m_cell     = _reader.ReadInt32();
            m_layer    = _reader.ReadInt32();
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            if( game_object == null )
                return;

            IActivationRangeTarget activation_range_target = game_object.GetComponent< IActivationRangeTarget >();
            if( activation_range_target == null )
                return;

            ActiveRangeSideScreen screen = Object.FindObjectOfType< ActiveRangeSideScreen >();
            if( screen != null )
            {
                Traverse traverse = Traverse.Create( screen );
                cActiveRangeSideScreenPatch.s_skip_send = true;
                if( m_active )
                {
                    traverse.Method( "OnActivateValueChanged", new[] { typeof( float ) } )?.GetValue( m_value );
                    KSlider slider = traverse.Field( "activateValueSlider" ).GetValue< KSlider >();
                    if( activation_range_target.ActivateValue >= activation_range_target.DeactivateValue )
                        slider.value = m_value;
                }
                else
                {
                    traverse.Method( "OnDeactivateValueChanged", new[] { typeof( float ) } )?.GetValue( m_value );
                    KSlider slider = traverse.Field( "deactivateValueSlider" ).GetValue< KSlider >();
                    if( activation_range_target.DeactivateValue <= activation_range_target.ActivateValue )
                        slider.value = m_value;
                }

                cActiveRangeSideScreenPatch.s_skip_send = false;
            }
            else
            {
                if( m_active )
                {
                    activation_range_target.ActivateValue = m_value;
                    if( activation_range_target.ActivateValue < activation_range_target.DeactivateValue )
                        activation_range_target.ActivateValue = activation_range_target.DeactivateValue;
                }
                else
                {
                    activation_range_target.DeactivateValue = m_value;
                    if( activation_range_target.DeactivateValue > activation_range_target.ActivateValue )
                        activation_range_target.DeactivateValue = activation_range_target.ActivateValue;
                }
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_value}, {m_value}, {m_cell}, {( Grid.SceneLayer )m_layer}" );
    }
}