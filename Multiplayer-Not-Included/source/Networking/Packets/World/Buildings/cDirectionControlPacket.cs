using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cDirectionControlPacket : iIPacket
    {
        private CSteamID                           m_steam_id = cSession.m_local_steam_id;
        private WorkableReactable.AllowedDirection m_direction;
        private int                                m_cell;
        private int                                m_layer;

        public ePacketType m_type => ePacketType.kDirectionControl;

        public cDirectionControlPacket() {}

        public cDirectionControlPacket( WorkableReactable.AllowedDirection _direction, int _cell, int _layer )
        {
            m_direction = _direction;
            m_cell      = _cell;
            m_layer     = _layer;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_direction );
            _writer.Write( m_cell );
            _writer.Write( m_layer );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id  = new CSteamID( _reader.ReadUInt64() );
            m_direction = ( WorkableReactable.AllowedDirection )_reader.ReadInt32();
            m_cell      = _reader.ReadInt32();
            m_layer     = _reader.ReadInt32();
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            if( game_object == null )
                return;

            DirectionControl direction_control = game_object.GetComponent< DirectionControl >();
            if( direction_control == null )
                return;

            Traverse.Create( direction_control ).Method( "SetAllowedDirection", new[] { typeof( WorkableReactable.AllowedDirection ) } )?.GetValue( m_direction );

            Game.Instance.userMenu.Refresh( game_object );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_direction}, {m_cell}, {( Grid.SceneLayer )m_layer}" );
    }
}