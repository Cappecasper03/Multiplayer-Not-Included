using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.source.Patches.Minions.Schedules;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded.Networking.Packets.Minions
{
    public class cSchedulePacket : iIPacket
    {
        private enum eAction
        {
            kAddDefault,
            kChangeName,
            kToggleAlarm,
            kDuplicate,
            kDelete,
            kDuplicateTimetable,
            kRemoveTimetable,
            kRotateTimetable,
            kShiftTimetable,
            kChangeAssignment,
            kChangeSchedule,
        }

        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private eAction  m_action;
        private string   m_name;
        private bool     m_alarm;
        private string   m_new_name;
        private int      m_timetable;
        private bool     m_rotate_direction;
        private string   m_schedule;
        private string   m_identity;
        private int      m_block_index;
        private string   m_group_name;
        private string   m_group_id;

        public ePacketType m_type => ePacketType.kSchedule;

        public static cSchedulePacket createDefault() => new cSchedulePacket { m_action = eAction.kAddDefault };

        public static cSchedulePacket createChangeName( string _name, string _new_name )
        {
            return new cSchedulePacket { m_action = eAction.kChangeName, m_name = _name, m_new_name = _new_name };
        }

        public static cSchedulePacket createToggleAlarm( string _name, bool _alarm )
        {
            return new cSchedulePacket { m_action = eAction.kToggleAlarm, m_name = _name, m_alarm = _alarm };
        }

        public static cSchedulePacket createDuplicate( string _name ) => new cSchedulePacket { m_action = eAction.kDuplicate, m_name = _name };
        public static cSchedulePacket createDelete( string    _name ) => new cSchedulePacket { m_action = eAction.kDelete, m_name    = _name };

        public static cSchedulePacket createDuplicateTimetable( string _name, int _timetable )
        {
            return new cSchedulePacket { m_action = eAction.kDuplicateTimetable, m_name = _name, m_timetable = _timetable };
        }

        public static cSchedulePacket createRemoveTimetable( string _name, int _timetable )
        {
            return new cSchedulePacket { m_action = eAction.kRemoveTimetable, m_name = _name, m_timetable = _timetable };
        }

        public static cSchedulePacket createRotateTimetable( string _name, int _timetable, bool _rotate_left )
        {
            return new cSchedulePacket { m_action = eAction.kRotateTimetable, m_name = _name, m_timetable = _timetable, m_rotate_direction = _rotate_left };
        }

        public static cSchedulePacket createShiftTimetable( string _name, int _timetable, bool _rotate_up )
        {
            return new cSchedulePacket { m_action = eAction.kShiftTimetable, m_name = _name, m_timetable = _timetable, m_rotate_direction = _rotate_up };
        }

        public static cSchedulePacket createChangeAssignment( string _name, string _schedule, string _identity )
        {
            return new cSchedulePacket { m_action = eAction.kChangeAssignment, m_name = _name, m_schedule = _schedule, m_identity = _identity };
        }

        public static cSchedulePacket createChangeSchedule( string _name, int _block_index, string _group_name, string _group_id )
        {
            return new cSchedulePacket { m_action = eAction.kChangeSchedule, m_name = _name, m_block_index = _block_index, m_group_name = _group_name, m_group_id = _group_id };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );

            if( m_action != eAction.kAddDefault )
                _writer.Write( m_name );

            switch( m_action )
            {
                case eAction.kChangeName:  _writer.Write( m_new_name ); break;
                case eAction.kToggleAlarm: _writer.Write( m_alarm ); break;
                case eAction.kDuplicateTimetable:
                case eAction.kRemoveTimetable: _writer.Write( m_timetable ); break;
                case eAction.kRotateTimetable:
                case eAction.kShiftTimetable:
                {
                    _writer.Write( m_timetable );
                    _writer.Write( m_rotate_direction );
                    break;
                }
                case eAction.kChangeAssignment:
                {
                    _writer.Write( m_schedule );
                    _writer.Write( m_identity );
                    break;
                }
                case eAction.kChangeSchedule:
                {
                    _writer.Write( m_block_index );
                    _writer.Write( m_group_name );
                    _writer.Write( m_group_id );
                    break;
                }
            }
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_action   = ( eAction )_reader.ReadInt32();

            if( m_action != eAction.kAddDefault )
                m_name = _reader.ReadString();

            switch( m_action )
            {
                case eAction.kChangeName:  m_new_name = _reader.ReadString(); break;
                case eAction.kToggleAlarm: m_alarm    = _reader.ReadBoolean(); break;
                case eAction.kDuplicateTimetable:
                case eAction.kRemoveTimetable: m_timetable = _reader.ReadInt32(); break;
                case eAction.kRotateTimetable:
                case eAction.kShiftTimetable:
                {
                    m_timetable        = _reader.ReadInt32();
                    m_rotate_direction = _reader.ReadBoolean();
                    break;
                }
                case eAction.kChangeAssignment:
                {
                    m_schedule = _reader.ReadString();
                    m_identity = _reader.ReadString();
                    break;
                }
                case eAction.kChangeSchedule:
                {
                    m_block_index = _reader.ReadInt32();
                    m_group_name  = _reader.ReadString();
                    m_group_id    = _reader.ReadString();
                    break;
                }
            }
        }

        public void onReceived()
        {
            switch( m_action )
            {
                case eAction.kAddDefault:         addDefault(); break;
                case eAction.kChangeName:         changeName(); break;
                case eAction.kToggleAlarm:        toggleAlarm(); break;
                case eAction.kDuplicate:          duplicate(); break;
                case eAction.kDelete:             delete(); break;
                case eAction.kDuplicateTimetable: duplicateTimetable(); break;
                case eAction.kRemoveTimetable:    removeTimetable(); break;
                case eAction.kRotateTimetable:    rotateTimetable(); break;
                case eAction.kShiftTimetable:     shiftTimetable(); break;
                case eAction.kChangeAssignment:   changeAssignment(); break;
                case eAction.kChangeSchedule:     changeSchedule(); break;
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kAddDefault:  cLogger.logInfo( $"{_message}: {m_action}" ); break;
                case eAction.kChangeName:  cLogger.logInfo( $"{_message}: {m_action}, {m_name}, {m_new_name}" ); break;
                case eAction.kToggleAlarm: cLogger.logInfo( $"{_message}: {m_action}, {m_name}, {m_alarm}" ); break;
                case eAction.kDuplicate:   cLogger.logInfo( $"{_message}: {m_action}, {m_name}, {m_timetable}" ); break;
                case eAction.kDelete:      cLogger.logInfo( $"{_message}: {m_action}, {m_name}" ); break;
                case eAction.kDuplicateTimetable:
                case eAction.kRemoveTimetable: cLogger.logInfo( $"{_message}: {m_action}, {m_name}, {m_timetable}" ); break;
                case eAction.kRotateTimetable:
                case eAction.kShiftTimetable: cLogger.logInfo( $"{_message}: {m_action}, {m_name}, {m_timetable}, {m_rotate_direction}" ); break;
                case eAction.kChangeAssignment: cLogger.logInfo( $"{_message}: {m_action}, {m_name}, {m_schedule}, {m_identity}" ); break;
                case eAction.kChangeSchedule:   cLogger.logInfo( $"{_message}: {m_action}, {m_name}, {m_block_index}, {m_group_name}, {m_group_id}" ); break;
            }
        }

        private static void addDefault()
        {
            cScheduleScreenPatch.s_skip_sending = true;
            Traverse.Create( ManagementMenu.Instance.scheduleScreen ).Method( "OnAddScheduleClick" )?.GetValue();
            cScheduleScreenPatch.s_skip_sending = false;
        }

        private void changeName()
        {
            ScheduleScreenEntry entry = findEntry( m_name );
            if( entry == null )
                return;

            EditableTitleBar title = Traverse.Create( entry ).Field( "title" ).GetValue< EditableTitleBar >();
            if( title == null )
                return;

            cScheduleScreenEntryPatch.s_skip_sending = true;
            Traverse.Create( title ).Method( "OnEndEdit", new[] { typeof( string ) } )?.GetValue( m_new_name );
            cScheduleScreenEntryPatch.s_skip_sending = false;
        }

        private void toggleAlarm()
        {
            ScheduleScreenEntry entry = findEntry( m_name );
            if( entry == null || entry.schedule.alarmActivated == m_alarm )
                return;

            cScheduleScreenEntryPatch.s_skip_sending = true;
            Traverse.Create( entry ).Method( "OnAlarmClicked" )?.GetValue();
            cScheduleScreenEntryPatch.s_skip_sending = false;
        }

        private void duplicate()
        {
            ScheduleScreenEntry entry = findEntry( m_name );
            if( entry == null )
                return;

            cScheduleScreenEntryPatch.s_skip_sending = true;
            Traverse.Create( entry ).Method( "DuplicateSchedule" )?.GetValue();
            cScheduleScreenEntryPatch.s_skip_sending = false;
        }

        private void delete()
        {
            ScheduleScreenEntry entry = findEntry( m_name );
            if( entry == null )
                return;

            cScheduleScreenEntryPatch.s_skip_sending = true;
            Traverse.Create( entry ).Method( "DeleteSchedule" )?.GetValue();
            cScheduleScreenEntryPatch.s_skip_sending = false;
        }

        private void duplicateTimetable()
        {
            ScheduleScreenEntry entry = findEntry( m_name );
            if( entry == null )
                return;

            cScheduleScreenEntryPatch.s_skip_sending = true;
            Traverse.Create( entry ).Method( "DuplicateTimetableRow", new[] { typeof( int ) } )?.GetValue( m_timetable );
            cScheduleScreenEntryPatch.s_skip_sending = false;
        }

        private void removeTimetable()
        {
            ScheduleScreenEntry entry = findEntry( m_name );
            if( entry == null )
                return;

            List< GameObject > timetable_rows = Traverse.Create( entry ).Field( "timetableRows" ).GetValue< List< GameObject > >();
            if( timetable_rows == null || m_timetable > timetable_rows.Count - 1 )
                return;

            cScheduleScreenEntryPatch.s_skip_sending = true;
            Traverse.Create( entry ).Method( "RemoveTimetableRow", new[] { typeof( GameObject ) } )?.GetValue( timetable_rows[ m_timetable ] );
            cScheduleScreenEntryPatch.s_skip_sending = false;
        }

        private void rotateTimetable()
        {
            ScheduleScreenEntry entry = findEntry( m_name );
            if( entry == null )
                return;

            List< GameObject > timetable_rows = Traverse.Create( entry ).Field( "timetableRows" ).GetValue< List< GameObject > >();
            if( timetable_rows == null || m_timetable / 24 > timetable_rows.Count - 1 )
                return;

            cSchedulePatch.s_skip_sending = true;
            entry.schedule.RotateBlocks( m_rotate_direction, m_timetable );
            cSchedulePatch.s_skip_sending = false;
        }

        private void shiftTimetable()
        {
            ScheduleScreenEntry entry = findEntry( m_name );
            if( entry == null )
                return;

            List< GameObject > timetable_rows = Traverse.Create( entry ).Field( "timetableRows" ).GetValue< List< GameObject > >();
            if( timetable_rows == null || m_timetable / 24 > timetable_rows.Count - 1 )
                return;

            cSchedulePatch.s_skip_sending = true;
            entry.schedule.ShiftTimetable( m_rotate_direction, m_timetable );
            cSchedulePatch.s_skip_sending = false;
        }

        private void changeAssignment()
        {
            ScheduleScreenEntry entry = findEntry( m_schedule );
            if( entry == null )
                return;

            foreach( Schedulable schedulable in Object.FindObjectsOfType< Schedulable >() )
            {
                if( schedulable.name != m_identity )
                    continue;

                cScheduleMinionWidgetPatch.s_skip_sending = true;
                ScheduleMinionWidget[] widgets = Object.FindObjectsOfType< ScheduleMinionWidget >();
                if( widgets.Length > 0 )
                {
                    foreach( ScheduleMinionWidget widget in widgets )
                    {
                        if( string.IsNullOrEmpty( m_name ) )
                        {
                            if( widget.schedulable != null )
                                continue;
                        }
                        else
                        {
                            if( widget.schedulable == null || widget.schedulable.name != m_name )
                                continue;
                        }

                        widget.ChangeAssignment( entry.schedule, schedulable );
                        break;
                    }
                }
                else
                {
                    ScheduleManager.Instance.GetSchedule( schedulable ).Unassign( schedulable );
                    entry.schedule.Assign( schedulable );
                }

                cScheduleMinionWidgetPatch.s_skip_sending = false;

                break;
            }
        }

        private void changeSchedule()
        {
            ScheduleScreenEntry entry = findEntry( m_name );

            cSchedulePatch.s_skip_sending = true;
            entry.schedule.SetBlockGroup( m_block_index, new ScheduleGroup( m_group_id, null, 0, m_group_name, "", Color.black, "", new List< ScheduleBlockType >() ) );
            cSchedulePatch.s_skip_sending = false;
        }

        private static ScheduleScreenEntry findEntry( string _name )
        {
            ScheduleScreen              screen  = ManagementMenu.Instance.scheduleScreen;
            List< ScheduleScreenEntry > entries = Traverse.Create( screen ).Field( "scheduleEntries" ).GetValue< List< ScheduleScreenEntry > >();

            if( entries == null )
            {
                object dictionary_object = Traverse.Create( ManagementMenu.Instance ).Field( "ScreenInfoMatch" ).GetValue();
                object info_object       = Traverse.Create( ManagementMenu.Instance ).Field( "scheduleInfo" ).GetValue();

                var                                     dictionary = dictionary_object as Dictionary< ManagementMenu.ManagementMenuToggleInfo, ManagementMenu.ScreenData >;
                ManagementMenu.ManagementMenuToggleInfo info       = info_object as ManagementMenu.ManagementMenuToggleInfo;
                if( dictionary == null || info == null )
                    return null;

                ManagementMenu.ScreenData screen_data = dictionary[ info ];

                // Forces the screen to initialize
                ManagementMenu.Instance.ToggleScreen( screen_data );
                screen.Start();
                ManagementMenu.Instance.ToggleScreen( screen_data );

                entries = Traverse.Create( screen ).Field( "scheduleEntries" ).GetValue< List< ScheduleScreenEntry > >();
                if( entries == null )
                    return null;
            }

            foreach( ScheduleScreenEntry entry in entries )
            {
                if( entry.schedule.name == _name )
                    return entry;
            }

            return null;
        }
    }
}