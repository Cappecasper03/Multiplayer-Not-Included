using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Creatures;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cAttackToolPacket : iIPacket
    {
        private CSteamID        m_steam_id = cSession.m_local_steam_id;
        private Vector2         m_min;
        private Vector2         m_max;
        private PrioritySetting m_priority = ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority();

        public ePacketType m_type => ePacketType.kAttackTool;

        public cAttackToolPacket() {}

        public cAttackToolPacket( Vector2 _min, Vector2 _max )
        {
            m_min = _min;
            m_max = _max;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_min );
            _writer.Write( m_max );
            _writer.Write( ( int )m_priority.priority_class );
            _writer.Write( m_priority.priority_value );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_min      = _reader.ReadVector2();
            m_max      = _reader.ReadVector2();
            m_priority = new PrioritySetting( ( PriorityScreen.PriorityClass )_reader.ReadInt32(), _reader.ReadInt32() );
        }

        public void onReceived()
        {
            Traverse        last_selected_priority = Traverse.Create( ToolMenu.Instance.PriorityScreen ).Field( "lastSelectedPriority" );
            PrioritySetting priority_setting       = last_selected_priority.GetValue< PrioritySetting >();

            last_selected_priority.SetValue( m_priority );

            cFactionAlignmentPatch.s_skip_send = true;
            AttackTool.MarkForAttack( m_min, m_max, true );
            cFactionAlignmentPatch.s_skip_send = false;

            last_selected_priority.SetValue( priority_setting );

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_min}, {m_max}" );
    }
}