using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.Tool;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cCancelToolPacket : iIPacket
    {
        private enum eAction
        {
            kCell,
            kArea,
        }

        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private eAction  m_action;
        private int      m_cell;
        private Vector3  m_down_pos;
        private Vector3  m_up_pos;

        public ePacketType m_type => ePacketType.kCancelTool;

        public static cCancelToolPacket createCell( int _cell ) => new cCancelToolPacket { m_action = eAction.kCell, m_cell = _cell };

        public static cCancelToolPacket createArea( Vector3 _down_pos, Vector3 _up_pos )
        {
            return new cCancelToolPacket { m_action = eAction.kArea, m_down_pos = _down_pos, m_up_pos = _up_pos };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );

            switch( m_action )
            {
                case eAction.kCell: _writer.Write( m_cell ); break;
                case eAction.kArea:
                {
                    _writer.Write( m_down_pos );
                    _writer.Write( m_up_pos );
                    break;
                }
            }
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_action   = ( eAction )_reader.ReadInt32();

            switch( m_action )
            {
                case eAction.kCell: m_cell = _reader.ReadInt32(); break;
                case eAction.kArea:
                {
                    m_down_pos = _reader.ReadVector3();
                    m_up_pos   = _reader.ReadVector3();
                    break;
                }
            }
        }

        public void onReceived()
        {
            cCancelToolPatch.s_skip_sending = true;
            switch( m_action )
            {
                case eAction.kCell: Traverse.Create( CancelTool.Instance ).Method( "OnDragTool", new[] { typeof( int ), typeof( int ) } )?.GetValue( m_cell, 0 ); break;
                case eAction.kArea:
                {
                    Traverse.Create( CancelTool.Instance ).Method( "OnDragComplete", new[] { typeof( Vector3 ), typeof( Vector3 ) } )?.GetValue( m_down_pos, m_up_pos );
                    break;
                }
            }

            cCancelToolPatch.s_skip_sending = false;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kCell: cLogger.logInfo( $"{_message}: {m_action}, {m_cell}" ); break;
                case eAction.kArea: cLogger.logInfo( $"{_message}: {m_action}, {m_down_pos}, {m_up_pos}" ); break;
            }
        }
    }
}