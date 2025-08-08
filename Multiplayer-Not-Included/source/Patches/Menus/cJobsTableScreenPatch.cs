using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;
using UnityEngine;

namespace MultiplayerNotIncluded.source.Patches.Menus
{
    [HarmonyPatch]
    public static class cJobsTableScreenPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( JobsTableScreen ), "ChangePersonalPriority" )]
        [HarmonyPatch( new[] { typeof( object ), typeof( int ) } )]
        private static void changePersonalPriority( object widget_go_obj, int delta, JobsTableScreen __instance )
        {
            if( !cSession.inSession() )
                return;

            MethodInfo get_widget_row    = __instance.GetType().GetMethod( "GetWidgetRow",    BindingFlags.NonPublic | BindingFlags.Instance );
            MethodInfo get_widget_column = __instance.GetType().GetMethod( "GetWidgetColumn", BindingFlags.NonPublic | BindingFlags.Instance );

            GameObject                     widget_go     = widget_go_obj as GameObject;
            TableRow                       widget_row    = get_widget_row?.Invoke( __instance, new object[] { widget_go } ) as TableRow;
            PrioritizationGroupTableColumn widget_column = get_widget_column?.Invoke( __instance, new object[] { widget_go } ) as PrioritizationGroupTableColumn;

            ChoreGroup chore_group = widget_column?.userData as ChoreGroup;
            if( widget_row == null || chore_group == null )
                return;

            List< string > identities = new List< string >();
            if( widget_row.rowType == TableRow.RowType.Default )
                identities.Add( "default" );
            else if( widget_row.GetIdentity() != null )
                identities.Add( widget_row.GetIdentity().GetProperName() );

            cJobPriorityPacket packet = cJobPriorityPacket.createPersonal( delta, new List< int > { chore_group.IdHash.HashValue }, identities );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( JobsTableScreen ), "ChangeColumnPriority" )]
        [HarmonyPatch( new[] { typeof( object ), typeof( int ) } )]
        private static void changeColumnPriority( object widget_go_obj, int new_priority, JobsTableScreen __instance )
        {
            if( !cSession.inSession() )
                return;

            MethodInfo get_widget_column = __instance.GetType().GetMethod( "GetWidgetColumn", BindingFlags.NonPublic | BindingFlags.Instance );

            GameObject                     widget_go     = widget_go_obj as GameObject;
            PrioritizationGroupTableColumn widget_column = get_widget_column?.Invoke( __instance, new object[] { widget_go } ) as PrioritizationGroupTableColumn;

            ChoreGroup chore_group = widget_column?.userData as ChoreGroup;
            if( chore_group == null )
                return;

            List< string > identities = new List< string >();
            foreach( TableRow table_row in __instance.rows )
            {
                if( table_row.rowType == TableRow.RowType.Default )
                    identities.Add( "default" );
                else if( table_row.GetIdentity() != null )
                    identities.Add( table_row.GetIdentity().GetProperName() );
            }

            cJobPriorityPacket packet = cJobPriorityPacket.createColumn( new_priority, new List< int > { chore_group.IdHash.HashValue }, identities );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( JobsTableScreen ), "ChangeRowPriority" )]
        [HarmonyPatch( new[] { typeof( object ), typeof( int ) } )]
        private static void changeRowPriority( object widget_go_obj, int delta, JobsTableScreen __instance )
        {
            if( !cSession.inSession() )
                return;

            MethodInfo get_widget_row = __instance.GetType().GetMethod( "GetWidgetRow", BindingFlags.NonPublic | BindingFlags.Instance );

            Dictionary< string, TableColumn > columns = AccessTools.Field( typeof( JobsTableScreen ), "columns" ).GetValue( __instance ) as Dictionary< string, TableColumn >;

            GameObject widget_go  = widget_go_obj as GameObject;
            TableRow   widget_row = get_widget_row?.Invoke( __instance, new object[] { widget_go } ) as TableRow;
            if( widget_row == null || columns == null )
                return;

            List< int > chore_group_ids = new List< int >();
            foreach( TableColumn table_column in columns.Values )
            {
                PrioritizationGroupTableColumn column      = table_column as PrioritizationGroupTableColumn;
                ChoreGroup                     chore_group = column?.userData as ChoreGroup;
                if( chore_group == null )
                    continue;

                chore_group_ids.Add( chore_group.IdHash.HashValue );
            }

            List< string > identities = new List< string >();
            if( widget_row.rowType == TableRow.RowType.Default )
                identities.Add( "default" );
            else if( widget_row.GetIdentity() != null )
                identities.Add( widget_row.GetIdentity().GetProperName() );

            cJobPriorityPacket packet = cJobPriorityPacket.createRow( delta, chore_group_ids, identities );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( JobsTableScreen ), "OnResetSettingsClicked" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onResetSettingsClicked()
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            cJobPriorityPacket packet = cJobPriorityPacket.createReset();

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( JobsTableScreen ), "OnAdvancedModeToggleClicked" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onAdvancedModeToggleClicked()
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            cJobPriorityPacket packet = cJobPriorityPacket.createToggleAdvanced( Convert.ToInt32( Game.Instance.advancedPersonalPriorities ) );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}