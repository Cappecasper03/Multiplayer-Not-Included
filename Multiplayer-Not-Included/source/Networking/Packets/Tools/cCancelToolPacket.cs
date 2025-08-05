using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cCancelToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_is_cell;
        private int      m_cell;
        private Vector2  m_min;
        private Vector2  m_max;

        public ePacketType m_type => ePacketType.kCancelTool;

        public cCancelToolPacket() {}

        public cCancelToolPacket( int _cell )
        {
            m_is_cell = true;
            m_cell    = _cell;
        }

        public cCancelToolPacket( Vector2 _min, Vector2 _max )
        {
            m_is_cell = false;
            m_min     = _min;
            m_max     = _max;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_is_cell );

            if( m_is_cell )
                _writer.Write( m_cell );
            else
            {
                _writer.Write( m_min );
                _writer.Write( m_max );
            }
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_is_cell  = _reader.ReadBoolean();

            if( m_is_cell )
                m_cell = _reader.ReadInt32();
            else
            {
                m_min = _reader.ReadVector2();
                m_max = _reader.ReadVector2();
            }
        }

        public void onDispatched()
        {
            if( !Grid.IsValidCell( m_cell ) )
                return;

            if( m_is_cell )
            {
                for( int i = 0; i < 45; ++i )
                {
                    GameObject game_object = Grid.Objects[ m_cell, i ];
                    if( game_object == null )
                        continue;

                    string filter = CancelTool.Instance?.GetFilterLayerFromGameObject( game_object );
                    if( filter == null || !CancelTool.Instance.IsActiveLayer( filter ) )
                        continue;

                    game_object.Trigger( ( int )GameHashes.Cancel );
                }
            }
            else
            {
                AttackTool.MarkForAttack( m_min, m_max, false );
                CaptureTool.MarkForCapture( m_min, m_max, false );
            }

            if( !cSession.isHost() )
                return;

            cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }
    }
}