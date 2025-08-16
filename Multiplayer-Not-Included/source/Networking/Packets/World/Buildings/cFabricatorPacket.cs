using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cFabricatorPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_set;
        private string   m_recipe_id;
        private int      m_count;
        private int      m_cell;
        private int      m_layer;

        public ePacketType m_type => ePacketType.kFabricator;

        public cFabricatorPacket() {}

        public cFabricatorPacket( bool _set, string _recipe_id, int _count, int _cell, int _layer )
        {
            m_set       = _set;
            m_recipe_id = _recipe_id;
            m_count     = _count;
            m_cell      = _cell;
            m_layer     = _layer;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_set );
            _writer.Write( m_recipe_id );
            _writer.Write( m_count );
            _writer.Write( m_cell );
            _writer.Write( m_layer );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id  = new CSteamID( _reader.ReadUInt64() );
            m_set       = _reader.ReadBoolean();
            m_recipe_id = _reader.ReadString();
            m_count     = _reader.ReadInt32();
            m_cell      = _reader.ReadInt32();
            m_layer     = _reader.ReadInt32();
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            if( game_object == null )
                return;

            ComplexFabricator complex_fabricator = game_object.GetComponent< ComplexFabricator >();
            if( complex_fabricator == null )
                return;

            ComplexRecipe complex_recipe = complex_fabricator.GetRecipe( m_recipe_id );
            if( complex_recipe == null )
                return;

            cComplexFabricatorPatch.s_skip_send = true;
            if( m_set )
                complex_fabricator.SetRecipeQueueCount( complex_recipe, m_count );
            else
            {
                if( m_count > 0 )
                    complex_fabricator.IncrementRecipeQueueCount( complex_recipe );
                else
                    complex_fabricator.DecrementRecipeQueueCount( complex_recipe );
            }

            cComplexFabricatorPatch.s_skip_send = false;

            SelectedRecipeQueueScreen queue_screen = Object.FindObjectOfType< SelectedRecipeQueueScreen >();
            if( queue_screen != null )
                Traverse.Create( queue_screen ).Method( "RefreshQueueCountDisplay" )?.GetValue();

            ComplexFabricatorSideScreen fabricator_screen = Object.FindObjectOfType< ComplexFabricatorSideScreen >();
            if( fabricator_screen != null )
                fabricator_screen.RefreshQueueCountDisplayForRecipeCategory( m_recipe_id, complex_fabricator );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_set}, {m_recipe_id}, {m_count}, {m_cell}, {( Grid.SceneLayer )m_layer}" );
    }
}