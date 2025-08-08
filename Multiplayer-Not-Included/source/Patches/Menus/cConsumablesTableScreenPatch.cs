using System;
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
    public static class cConsumablesTableScreenPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ConsumablesTableScreen ), "set_value_consumable_info" )]
        [HarmonyPatch( new[] { typeof( GameObject ), typeof( TableScreen.ResultValues ) } )]
        private static void setValueConsumableInfo( GameObject widget_go, TableScreen.ResultValues new_value, ConsumablesTableScreen __instance )
        {
            if( !cSession.inSession() )
                return;

            MethodInfo get_widget_row    = __instance.GetType().GetMethod( "GetWidgetRow",    BindingFlags.NonPublic | BindingFlags.Instance );
            MethodInfo get_widget_column = __instance.GetType().GetMethod( "GetWidgetColumn", BindingFlags.NonPublic | BindingFlags.Instance );

            TableRow                  widget_row    = get_widget_row?.Invoke( __instance, new object[] { widget_go } ) as TableRow;
            ConsumableInfoTableColumn widget_column = get_widget_column?.Invoke( __instance, new object[] { widget_go } ) as ConsumableInfoTableColumn;
            if( widget_row == null || widget_column == null )
                return;

            IConsumableUIItem   consumable_info = widget_column.consumable_info;
            IAssignableIdentity identity        = widget_row.GetIdentity();
            string              identity_name   = identity != null ? identity.GetProperName() : "None";

            cConsumableInfoPacket packet = new cConsumableInfoPacket( widget_row.rowType, consumable_info.ConsumableId, new_value, identity_name );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}