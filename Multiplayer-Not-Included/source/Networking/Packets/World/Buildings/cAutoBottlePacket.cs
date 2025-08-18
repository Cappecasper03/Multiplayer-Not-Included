using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cAutoBottlePacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_auto_bottle;
        private int      m_cell;
        private int      m_layer;

        public ePacketType m_type => ePacketType.kAutoBottle;

        public cAutoBottlePacket() {}

        public cAutoBottlePacket( bool _auto_bottle, int _cell, int _layer )
        {
            m_auto_bottle = _auto_bottle;
            m_cell        = _cell;
            m_layer       = _layer;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_auto_bottle );
            _writer.Write( m_cell );
            _writer.Write( m_layer );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_auto_bottle = _reader.ReadBoolean();
            m_cell        = _reader.ReadInt32();
            m_layer       = _reader.ReadInt32();
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            if( game_object == null )
                return;

            BottleEmptier bottle_emptier = game_object.GetComponent< BottleEmptier >();
            if( bottle_emptier == null )
                return;

            if( bottle_emptier.allowManualPumpingStationFetching != m_auto_bottle )
                return;

            cBottleEmptierPatch.s_skip_send = true;
            Traverse.Create( bottle_emptier ).Method( "OnChangeAllowManualPumpingStationFetching" )?.GetValue();
            cBottleEmptierPatch.s_skip_send = false;

            Game.Instance.userMenu.Refresh( game_object );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_auto_bottle}, {m_cell}, {( Grid.SceneLayer )m_layer}" );
    }
}