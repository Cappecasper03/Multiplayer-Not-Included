using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.Tool;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cClearToolPacket : iIPacket
    {
        private CSteamID        m_steam_id = cSession.m_local_steam_id;
        private int             m_cell;
        private PrioritySetting m_priority = ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority();

        public ePacketType m_type => ePacketType.kClearTool;

        public cClearToolPacket() {}

        public cClearToolPacket( int _cell ) => m_cell = _cell;

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_cell );
            _writer.Write( ( int )m_priority.priority_class );
            _writer.Write( m_priority.priority_value );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_cell     = _reader.ReadInt32();
            m_priority = new PrioritySetting( ( PriorityScreen.PriorityClass )_reader.ReadInt32(), _reader.ReadInt32() );
        }

        public void onReceived()
        {
            Traverse        last_selected_priority = Traverse.Create( ToolMenu.Instance.PriorityScreen ).Field( "lastSelectedPriority" );
            PrioritySetting priority_setting       = last_selected_priority.GetValue< PrioritySetting >();

            last_selected_priority.SetValue( m_priority );

            cClearToolPatch.s_skip_sending = true;
            Traverse.Create( ClearTool.Instance ).Method( "OnDragTool", new[] { typeof( int ), typeof( int ) } )?.GetValue( m_cell, 0 );
            cClearToolPatch.s_skip_sending = false;

            last_selected_priority.SetValue( priority_setting );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_cell}" );
    }
}