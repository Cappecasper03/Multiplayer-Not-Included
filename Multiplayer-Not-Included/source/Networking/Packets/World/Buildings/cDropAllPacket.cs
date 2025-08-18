using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cDropAllPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_drop;
        private int      m_cell;
        private int      m_layer;

        public ePacketType m_type => ePacketType.kDropAll;

        public cDropAllPacket() {}

        public cDropAllPacket( bool _drop, int _cell, int _layer )
        {
            m_drop  = _drop;
            m_cell  = _cell;
            m_layer = _layer;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_drop );
            _writer.Write( m_cell );
            _writer.Write( m_layer );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_drop     = _reader.ReadBoolean();
            m_cell     = _reader.ReadInt32();
            m_layer    = _reader.ReadInt32();
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            if( game_object == null )
                return;

            DropAllWorkable drop_all_workable = game_object.GetComponent< DropAllWorkable >();
            if( drop_all_workable == null )
                return;

            if( m_drop != Traverse.Create( drop_all_workable ).Field( "markedForDrop" ).GetValue< bool >() )
                drop_all_workable.DropAll();

            Game.Instance.userMenu.Refresh( game_object );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_drop}, {m_cell}, {( Grid.SceneLayer )m_layer}" );
    }
}