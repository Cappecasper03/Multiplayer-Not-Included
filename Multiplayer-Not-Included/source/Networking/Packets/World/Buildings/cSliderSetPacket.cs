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
    public class cSliderSetPacket : iIPacket
    {
        public enum eAction
        {
            kNone,
            kSingle,
            kDual,
            kMulti,
            kInt,
        }

        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private eAction  m_action;
        private float    m_value;
        private int      m_cell;
        private int      m_layer;

        public ePacketType m_type => ePacketType.kSliderSet;

        public cSliderSetPacket() {}

        public cSliderSetPacket( eAction _action, float _value, int _cell, int _layer )
        {
            m_action = _action;
            m_value  = _value;
            m_cell   = _cell;
            m_layer  = _layer;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_value );
            _writer.Write( m_cell );
            _writer.Write( m_layer );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_action   = ( eAction )_reader.ReadInt32();
            m_value    = _reader.ReadSingle();
            m_cell     = _reader.ReadInt32();
            m_layer    = _reader.ReadInt32();
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            if( game_object == null )
                return;

            ISliderControl slider_control = game_object.GetComponent< ISliderControl >();
            if( slider_control == null )
                return;

            SliderSet slider_set = null;
            cSliderSetPatch.s_skip_send = true;
            switch( m_action )
            {
                case eAction.kSingle: slider_set = findSliderSet< SingleSliderSideScreen >( slider_control ); break;
                case eAction.kDual:   slider_set = findSliderSet< DualSliderSideScreen >( slider_control ); break;
                case eAction.kMulti:  slider_set = findSliderSet< MultiSliderSideScreen >( slider_control ); break;
                case eAction.kInt:    slider_set = findSliderSet< IntSliderSideScreen >( slider_control ); break;
            }

            if( slider_set != null )
            {
                Traverse.Create( slider_set ).Method( "SetValue", new[] { typeof( float ) } )?.GetValue( m_value );
                slider_set.valueSlider.value = m_value;
            }
            else
                slider_control.SetSliderValue( m_value, 0 );

            cSliderSetPatch.s_skip_send = false;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_action}, {m_value}, {m_cell}, {( Grid.SceneLayer )m_layer}" );

        private static SliderSet findSliderSet< T >( ISliderControl _target )
            where T : SideScreenContent
        {
            T screen = Object.FindObjectOfType< T >();
            if( screen == null )
                return null;

            foreach( SliderSet slider_set in Traverse.Create( screen ).Field( "sliderSets" ).GetValue< List< SliderSet > >() )
            {
                ISliderControl target = Traverse.Create( slider_set ).Field( "target" ).GetValue< ISliderControl >();
                if( _target == target )
                    return slider_set;
            }

            return null;
        }
    }
}