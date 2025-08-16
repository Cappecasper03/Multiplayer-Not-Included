using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cFilterPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private Tag      m_filter;
        private int      m_cell;
        private int      m_layer;

        public ePacketType m_type => ePacketType.kFilter;

        public cFilterPacket() {}

        public cFilterPacket( Tag _filter, int _cell, int _layer )
        {
            m_filter = _filter;
            m_cell   = _cell;
            m_layer  = _layer;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_filter.Name );
            _writer.Write( m_cell );
            _writer.Write( m_layer );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_filter   = new Tag( _reader.ReadString() );
            m_cell     = _reader.ReadInt32();
            m_layer    = _reader.ReadInt32();
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            if( game_object == null )
                return;

            Filterable filterable = game_object.GetComponent< Filterable >();
            if( filterable == null )
                return;

            cFilterablePatch.s_skip_send = true;
            Traverse.Create( filterable ).Field( "selectedTag" )?.SetValue( m_filter );
            Traverse.Create( filterable ).Method( "OnFilterChanged" )?.GetValue();
            cFilterablePatch.s_skip_send = false;

            FilterSideScreen screen = Object.FindObjectOfType< FilterSideScreen >();
            if( screen != null )
                Traverse.Create( screen ).Method( "RefreshUI" )?.GetValue();

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_filter.Name}, {m_cell}, {( Grid.SceneLayer )m_layer}" );
    }
}