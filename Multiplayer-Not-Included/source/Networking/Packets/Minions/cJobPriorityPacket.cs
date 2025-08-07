using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.source.Patches.Menus;
using Steamworks;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded.Networking.Packets.Minions
{
    public class cJobPriorityPacket : iIPacket
    {
        private CSteamID       m_steam_id = cSession.m_local_steam_id;
        private int            m_priority;
        private List< int >    m_chore_type_ids = new List< int >();
        private List< string > m_identities     = new List< string >();

        public ePacketType m_type => ePacketType.kJobPriority;

        public cJobPriorityPacket() {}

        public cJobPriorityPacket( int _priority, List< int > _chore_type_ids, List< string > _identities )
        {
            m_priority       = _priority;
            m_chore_type_ids = _chore_type_ids;
            m_identities     = _identities;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_priority );

            _writer.Write( m_chore_type_ids.Count );
            foreach( int chore_type_id in m_chore_type_ids )
                _writer.Write( chore_type_id );

            _writer.Write( m_identities.Count );
            foreach( string identity in m_identities )
                _writer.Write( identity );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_priority = _reader.ReadInt32();

            int chore_type_id_count = _reader.ReadInt32();
            for( int i = 0; i < chore_type_id_count; i++ )
                m_chore_type_ids.Add( _reader.ReadInt32() );

            int identity_count = _reader.ReadInt32();
            for( int i = 0; i < identity_count; i++ )
                m_identities.Add( _reader.ReadString() );
        }

        public void onReceived()
        {
            List< ChoreGroup >               chore_groups      = findChoreGroups();
            List< IPersonalPriorityManager > priority_managers = findPriorityManagers();

            bool is_reset    = chore_groups.Count == 0 && priority_managers.Count == 0 && m_priority < 0;
            bool is_advanced = chore_groups.Count == 0 && priority_managers.Count == 0 && m_priority >= 0;
            bool is_personal = chore_groups.Count == 1 && priority_managers.Count == 1;
            bool is_column   = chore_groups.Count == 1 && priority_managers.Count >= 1;
            bool is_row      = chore_groups.Count >= 1 && priority_managers.Count == 1;

            cJobsTableScreenPatch.s_skip_sending = true;
            if( is_reset )
            {
                MethodInfo method_info = ManagementMenu.Instance.jobsScreen.GetType().GetMethod( "OnResetSettingsClicked", BindingFlags.NonPublic | BindingFlags.Instance );
                method_info?.Invoke( ManagementMenu.Instance.jobsScreen, new object[] {} );
            }
            else if( is_advanced )
            {
                if( Game.Instance.advancedPersonalPriorities != Convert.ToBoolean( m_priority ) )
                {
                    MethodInfo method_info = ManagementMenu.Instance.jobsScreen.GetType().GetMethod( "OnAdvancedModeToggleClicked", BindingFlags.NonPublic | BindingFlags.Instance );
                    method_info?.Invoke( ManagementMenu.Instance.jobsScreen, new object[] {} );
                }
            }
            else
            {
                foreach( IPersonalPriorityManager priority_manager in priority_managers )
                {
                    foreach( ChoreGroup chore_group in chore_groups )
                    {
                        if( is_personal || is_row )
                            priority_manager.SetPersonalPriority( chore_group, priority_manager.GetPersonalPriority( chore_group ) + m_priority );
                        else if( is_column )
                            priority_manager.SetPersonalPriority( chore_group, m_priority );
                    }
                }
            }

            cJobsTableScreenPatch.s_skip_sending = false;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_steam_id}" );

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

            if( m_identities.Count == 0 )
                return priority_managers;

            if( m_identities.Contains( "default" ) )
            {
                priority_managers.Add( Immigration.Instance );

                if( m_identities.Count == 1 )
                    return priority_managers;
            }

            MinionIdentity[] minion_identities = Object.FindObjectsOfType< MinionIdentity >();
            foreach( MinionIdentity identity in minion_identities )
            {
                if( m_identities.Contains( identity.GetProperName() ) )
                    priority_managers.Add( identity.GetComponent< ChoreConsumer >() );
            }

            return priority_managers;
        }
    }
}