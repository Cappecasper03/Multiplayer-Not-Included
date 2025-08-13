using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.source.Networking.Components;
using MultiplayerNotIncluded.source.Patches.Minions;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.Minions
{
    public class cJobPriorityPacket : iIPacket
    {
        private enum eAction
        {
            kReset,
            kToggleAdvanced,
            kPersonal,
            kColumn,
            kRow,
        }

        private CSteamID    m_steam_id = cSession.m_local_steam_id;
        private eAction     m_action;
        private int         m_priority;
        private List< int > m_chore_type_ids = new List< int >();
        private List< int > m_network_ids    = new List< int >();

        public ePacketType m_type => ePacketType.kJobPriority;

        public static cJobPriorityPacket createReset()                         => new cJobPriorityPacket { m_action = eAction.kReset };
        public static cJobPriorityPacket createToggleAdvanced( int _priority ) => new cJobPriorityPacket { m_action = eAction.kToggleAdvanced, m_priority = _priority };

        public static cJobPriorityPacket createPersonal( int _priority, List< int > _chore_type_ids, List< int > _network_ids )
        {
            return new cJobPriorityPacket { m_action = eAction.kPersonal, m_priority = _priority, m_chore_type_ids = _chore_type_ids, m_network_ids = _network_ids };
        }

        public static cJobPriorityPacket createColumn( int _priority, List< int > _chore_type_ids, List< int > _network_ids )
        {
            return new cJobPriorityPacket { m_action = eAction.kColumn, m_priority = _priority, m_chore_type_ids = _chore_type_ids, m_network_ids = _network_ids };
        }

        public static cJobPriorityPacket createRow( int _priority, List< int > _chore_type_ids, List< int > _network_ids )
        {
            return new cJobPriorityPacket { m_action = eAction.kRow, m_priority = _priority, m_chore_type_ids = _chore_type_ids, m_network_ids = _network_ids };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );

            switch( m_action )
            {
                case eAction.kReset:          break;
                case eAction.kToggleAdvanced: _writer.Write( m_priority ); break;
                case eAction.kPersonal:
                case eAction.kColumn:
                case eAction.kRow:
                {
                    _writer.Write( m_priority );

                    _writer.Write( m_chore_type_ids.Count );
                    foreach( int chore_type_id in m_chore_type_ids )
                        _writer.Write( chore_type_id );

                    _writer.Write( m_network_ids.Count );
                    foreach( int identity in m_network_ids )
                        _writer.Write( identity );
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
                case eAction.kReset:          break;
                case eAction.kToggleAdvanced: m_priority = _reader.ReadInt32(); break;
                case eAction.kPersonal:
                case eAction.kColumn:
                case eAction.kRow:
                {
                    m_priority = _reader.ReadInt32();

                    int chore_type_id_count = _reader.ReadInt32();
                    for( int i = 0; i < chore_type_id_count; i++ )
                        m_chore_type_ids.Add( _reader.ReadInt32() );

                    int identity_count = _reader.ReadInt32();
                    for( int i = 0; i < identity_count; i++ )
                        m_network_ids.Add( _reader.ReadInt32() );
                    break;
                }
            }
        }

        public void onReceived()
        {
            cJobsTableScreenPatch.s_skip_sending = true;
            switch( m_action )
            {
                case eAction.kReset: Traverse.Create( ManagementMenu.Instance.jobsScreen ).Method( "OnResetSettingsClicked" )?.GetValue(); break;
                case eAction.kToggleAdvanced:
                {
                    if( Game.Instance.advancedPersonalPriorities != Convert.ToBoolean( m_priority ) )
                        Traverse.Create( ManagementMenu.Instance.jobsScreen ).Method( "OnAdvancedModeToggleClicked" )?.GetValue();

                    break;
                }
                case eAction.kPersonal:
                case eAction.kColumn:
                case eAction.kRow:
                {
                    List< ChoreGroup >               chore_groups      = findChoreGroups();
                    List< IPersonalPriorityManager > priority_managers = findPriorityManagers();

                    foreach( IPersonalPriorityManager priority_manager in priority_managers )
                    {
                        foreach( ChoreGroup chore_group in chore_groups )
                        {
                            if( m_action == eAction.kColumn )
                                priority_manager.SetPersonalPriority( chore_group, m_priority );
                            else
                                priority_manager.SetPersonalPriority( chore_group, priority_manager.GetPersonalPriority( chore_group ) + m_priority );
                        }
                    }

                    break;
                }
            }

            Traverse.Create( ManagementMenu.Instance.jobsScreen ).Method( "MarkRowsDirty" )?.GetValue();
            cJobsTableScreenPatch.s_skip_sending = false;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kReset:          cLogger.logInfo( $"{_message}: {m_action}" ); break;
                case eAction.kToggleAdvanced: cLogger.logInfo( $"{_message}: {m_action}, {m_priority}" ); break;
                case eAction.kPersonal:       cLogger.logInfo( $"{_message}: {m_action}, {m_priority}, {m_network_ids[ 0 ]}, {m_chore_type_ids[ 0 ]}" ); break;
                case eAction.kColumn:         cLogger.logInfo( $"{_message}: {m_action}, {m_priority}, {m_chore_type_ids[ 0 ]}" ); break;
                case eAction.kRow:            cLogger.logInfo( $"{_message}: {m_action}, {m_priority}, {m_network_ids[ 0 ]}" ); break;
            }
        }

        private List< ChoreGroup > findChoreGroups()
        {
            List< ChoreGroup > chore_roups = new List< ChoreGroup >();

            if( m_chore_type_ids.Count == 0 )
                return chore_roups;

            foreach( ChoreGroup chore in new List< ChoreGroup >( Db.Get().ChoreGroups.resources ) )
            {
                if( m_chore_type_ids.Contains( chore.IdHash.HashValue ) )
                    chore_roups.Add( chore );
            }

            return chore_roups;
        }

        private List< IPersonalPriorityManager > findPriorityManagers()
        {
            List< IPersonalPriorityManager > priority_managers = new List< IPersonalPriorityManager >();

            if( m_network_ids.Count == 0 )
                return priority_managers;

            if( m_network_ids.Contains( -1 ) )
            {
                priority_managers.Add( Immigration.Instance );

                if( m_network_ids.Count == 1 )
                    return priority_managers;
            }

            foreach( int network_id in m_network_ids )
            {
                cNetworkIdentity identity;
                if( cNetworkIdentity.tryGetIdentity( network_id, out identity ) )
                    priority_managers.Add( identity.GetComponent< ChoreConsumer >() );
            }

            return priority_managers;
        }
    }
}