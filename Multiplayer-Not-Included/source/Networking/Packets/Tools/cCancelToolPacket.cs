using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MultiplayerNotIncluded.Patches.Tool;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cCancelToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_is_cell;
        private int      m_cell;
        private Vector3  m_down_pos;
        private Vector3  m_up_pos;

        public ePacketType m_type => ePacketType.kCancelTool;

        public cCancelToolPacket() {}

        public cCancelToolPacket( int _cell )
        {
            m_is_cell = true;
            m_cell    = _cell;
        }

        public cCancelToolPacket( Vector3 _down_pos, Vector3 _up_pos )
        {
            m_is_cell  = false;
            m_down_pos = _down_pos;
            m_up_pos   = _up_pos;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_is_cell );

            if( m_is_cell )
                _writer.Write( m_cell );
            else
            {
                _writer.Write( m_down_pos );
                _writer.Write( m_up_pos );
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
                m_down_pos = _reader.ReadVector3();
                m_up_pos   = _reader.ReadVector3();
            }
        }

        public void onDispatched()
        {
            cCancelToolPatch.s_skip_sending = true;
            if( m_is_cell )
            {
                MethodInfo on_drag_tool = CancelTool.Instance.GetType().GetMethod( "OnDragTool", BindingFlags.NonPublic | BindingFlags.Instance );
                on_drag_tool?.Invoke( CancelTool.Instance, new object[] { m_cell, 0 } );
            }
            else
            {
                MethodInfo on_drag_complete = CancelTool.Instance.GetType().GetMethod( "OnDragComplete", BindingFlags.NonPublic | BindingFlags.Instance );
                on_drag_complete?.Invoke( CancelTool.Instance, new object[] { m_down_pos, m_up_pos } );
            }

            cCancelToolPatch.s_skip_sending = false;

            if( !cSession.isHost() )
                return;

            cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }
    }
}