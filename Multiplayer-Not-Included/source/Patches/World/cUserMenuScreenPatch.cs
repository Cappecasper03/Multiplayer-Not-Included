using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.World
{
    [HarmonyPatch]
    public static class cUserMenuScreenPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( UserMenuScreen ), "OnPriorityClicked" )]
        [HarmonyPatch( new[] { typeof( PrioritySetting ) } )]
        private static void onPriorityClicked( PrioritySetting priority, UserMenuScreen __instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            GameObject game_object = Traverse.Create( __instance ).Field( "selected" ).GetValue< GameObject >();
            KPrefabID  prefab_id   = game_object?.GetComponent< KPrefabID >();
            if( prefab_id == null )
                return;

            cPriorityPacket packet = new cPriorityPacket( priority, prefab_id.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}