using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cCancelToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private int      m_cell;

        public ePacketType m_type => ePacketType.kCancelTool;

        public cCancelToolPacket() {}

        public cCancelToolPacket( int _cell ) => m_cell = _cell;

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_cell );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_cell     = _reader.ReadInt32();
        }

        public void onDispatched()
        {
            if( !Grid.IsValidCell( m_cell ) )
                return;

            for( int i = 0; i < 45; ++i )
            {
                GameObject game_object = Grid.Objects[ m_cell, i ];
                if( game_object == null )
                    continue;

                string filter = CancelTool.Instance?.GetFilterLayerFromGameObject( game_object );
                if( filter == null || !CancelTool.Instance.IsActiveLayer( filter ) )
                    continue;

                game_object.Trigger( 2127324410 );
            }

            if( !cSession.isHost() )
                return;

            cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }
    }
}