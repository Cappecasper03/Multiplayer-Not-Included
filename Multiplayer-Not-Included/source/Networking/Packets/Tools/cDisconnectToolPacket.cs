using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.Tool;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cDisconnectToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private Vector3  m_down_pos;
        private Vector3  m_up_pos;

        public ePacketType m_type => ePacketType.kDisconnectTool;

        public cDisconnectToolPacket() {}

        public cDisconnectToolPacket( Vector3 _down_pos, Vector3 _up_pos )
        {
            m_down_pos = _down_pos;
            m_up_pos   = _up_pos;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_down_pos );
            _writer.Write( m_up_pos );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_down_pos = _reader.ReadVector3();
            m_up_pos   = _reader.ReadVector3();
        }

        public void onReceived()
        {
            cDisconnectToolPatch.s_skip_sending = true;
            Traverse single_disconnect_mode = Traverse.Create( DisconnectTool.Instance ).Field( "singleDisconnectMode" );
            bool     original_value         = single_disconnect_mode.GetValue< bool >();
            single_disconnect_mode.SetValue( false );
            Traverse.Create( DisconnectTool.Instance ).Method( "OnDragComplete", new[] { typeof( Vector3 ), typeof( Vector3 ) } )?.GetValue( m_down_pos, m_up_pos );
            single_disconnect_mode.SetValue( original_value );

            cDisconnectToolPatch.s_skip_sending = false;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_down_pos}, {m_up_pos}" );
    }
}