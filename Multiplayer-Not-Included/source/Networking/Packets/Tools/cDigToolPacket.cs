using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.Tool;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cDigToolPacket : iIPacket
    {
        private CSteamID        m_steam_id = cSession.m_local_steam_id;
        private int             m_cell;
        private int             m_animation_delay;
        private PrioritySetting m_priority = ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority();

        public ePacketType m_type => ePacketType.kDigTool;

        public cDigToolPacket() {}

        public cDigToolPacket( int _cell, int _animation_delay )
        {
            m_cell            = _cell;
            m_animation_delay = _animation_delay;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_cell );
            _writer.Write( m_animation_delay );
            _writer.Write( ( int )m_priority.priority_class );
            _writer.Write( m_priority.priority_value );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id        = new CSteamID( _reader.ReadUInt64() );
            m_cell            = _reader.ReadInt32();
            m_animation_delay = _reader.ReadInt32();
            m_priority        = new PrioritySetting( ( PriorityScreen.PriorityClass )_reader.ReadInt32(), _reader.ReadInt32() );
        }

        public void onReceived()
        {
            cDigToolPatch.s_skip_sending = true;
            GameObject game_object = DigTool.PlaceDig( m_cell, m_animation_delay );
            cDigToolPatch.s_skip_sending = false;

            Prioritizable prioritizable = game_object?.GetComponent< Prioritizable >();
            prioritizable?.SetMasterPriority( m_priority );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_cell}, {m_animation_delay}" );
    }
}