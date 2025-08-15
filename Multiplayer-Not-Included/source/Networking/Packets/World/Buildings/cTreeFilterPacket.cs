using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cTreeFilterPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_add;
        private Tag      m_tag;
        private int      m_cell;
        private int      m_layer;

        public ePacketType m_type => ePacketType.kTreeFilter;

        public cTreeFilterPacket() {}

        public cTreeFilterPacket( bool _add, Tag _tag, int _cell, int _layer )
        {
            m_add   = _add;
            m_tag   = _tag;
            m_cell  = _cell;
            m_layer = _layer;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_add );
            _writer.Write( m_tag.Name );
            _writer.Write( m_cell );
            _writer.Write( m_layer );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_add      = _reader.ReadBoolean();
            m_tag      = new Tag( _reader.ReadString() );
            m_cell     = _reader.ReadInt32();
            m_layer    = _reader.ReadInt32();
        }

        public void onReceived()
        {
            TreeFilterable tree_filterable = Grid.Objects[ m_cell, m_layer ]?.GetComponent< TreeFilterable >();
            if( tree_filterable == null )
                return;

            cTreeFilterablePatch.s_skip_send = true;
            if( m_add )
                tree_filterable.AddTagToFilter( m_tag );
            else
                tree_filterable.RemoveTagFromFilter( m_tag );

            TreeFilterableSideScreen screen = Object.FindObjectOfType< TreeFilterableSideScreen >();
            if( screen != null )
            {
                Traverse traverse = Traverse.Create( screen ).Field( "visualDirty" );
                if( traverse != null )
                {
                    traverse.SetValue( true );
                    screen.Update();
                    traverse.SetValue( false );
                }
            }

            cTreeFilterablePatch.s_skip_send = false;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_add}, {m_tag.Name}, {m_cell}, {( Grid.SceneLayer )m_layer}" );
    }
}