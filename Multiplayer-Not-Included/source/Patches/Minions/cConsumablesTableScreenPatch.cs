using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;
using UnityEngine;

namespace MultiplayerNotIncluded.source.Patches.Minions
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
            if( !cSession.inSessionAndReady() )
                return;

            Traverse get_widget_row    = Traverse.Create( __instance ).Method( "GetWidgetRow",    new[] { typeof( GameObject ) } );
            Traverse get_widget_column = Traverse.Create( __instance ).Method( "GetWidgetColumn", new[] { typeof( GameObject ) } );

            TableRow                  widget_row    = get_widget_row?.GetValue< TableRow >( widget_go );
            ConsumableInfoTableColumn widget_column = get_widget_column?.GetValue< ConsumableInfoTableColumn >( widget_go );
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