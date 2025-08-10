using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;
using UnityEngine;

namespace MultiplayerNotIncluded.source.Patches.Menus
{
    [HarmonyPatch]
    public static class cScheduleScreenEntryPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ScheduleScreenEntry ), "DuplicateSchedule" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void duplicateSchedule( ScheduleScreenEntry __instance )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            cSchedulePacket packet = cSchedulePacket.createDuplicate( __instance.schedule.name );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ScheduleScreenEntry ), "DeleteSchedule" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void deleteSchedule( ScheduleScreenEntry __instance )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            cSchedulePacket packet = cSchedulePacket.createDelete( __instance.schedule.name );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ScheduleScreenEntry ), "DuplicateTimetableRow" )]
        [HarmonyPatch( new[] { typeof( int ) } )]
        private static void duplicateTimetableRow( int sourceTimetableIdx, ScheduleScreenEntry __instance )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            cSchedulePacket packet = cSchedulePacket.createDuplicateTimetable( __instance.schedule.name, sourceTimetableIdx );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ScheduleScreenEntry ), "RemoveTimetableRow" )]
        [HarmonyPatch( new[] { typeof( GameObject ) } )]
        private static void removeTimetableRow( GameObject row, ScheduleScreenEntry __instance )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            List< GameObject > timetable_rows = Traverse.Create( __instance ).Field( "timetableRows" ).GetValue< List< GameObject > >();
            if( timetable_rows == null || timetable_rows.Count == 0 )
                return;

            int index = 0;
            foreach( GameObject timetable_row in timetable_rows )
            {
                if( timetable_row == row )
                    break;

                index++;
            }

            cSchedulePacket packet = cSchedulePacket.createRemoveTimetable( __instance.schedule.name, index );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPrefix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ScheduleScreenEntry ), "OnNameChanged" )]
        [HarmonyPatch( new[] { typeof( string ) } )]
        private static void onNameChanged( string newName, ScheduleScreenEntry __instance )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            cSchedulePacket packet = cSchedulePacket.createChangeName( __instance.schedule.name, newName );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ScheduleScreenEntry ), "OnAlarmClicked" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onAlarmClicked( ScheduleScreenEntry __instance )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            cSchedulePacket packet = cSchedulePacket.createToggleAlarm( __instance.schedule.name, __instance.schedule.alarmActivated );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}