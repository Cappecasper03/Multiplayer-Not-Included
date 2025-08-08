using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.Tool
{
    [HarmonyPatch]
    public static class cMoveToLocationToolPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( MoveToLocationTool ), "SetMoveToLocation" )]
        [HarmonyPatch( new[] { typeof( int ) } )]
        private static void setMoveToLocation( int target_cell, MoveToLocationTool __instance )
        {
            if( !cSession.inSession() )
                return;

            Navigator navigator = AccessTools.Field( typeof( MoveToLocationTool ), "targetNavigator" ).GetValue( __instance ) as Navigator;
            Movable   movable   = AccessTools.Field( typeof( MoveToLocationTool ), "targetMovable" ).GetValue( __instance ) as Movable;

            GameObject game_object = navigator?.gameObject ?? movable?.gameObject;
            KPrefabID  prefab_id   = game_object?.GetComponent< KPrefabID >();

            if( prefab_id == null )
                return;

            cMoveToLocationToolPacket packet = new cMoveToLocationToolPacket( target_cell, prefab_id.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}