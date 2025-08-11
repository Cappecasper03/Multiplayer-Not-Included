using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Plants;

namespace MultiplayerNotIncluded.Patches.World.Items
{
    [HarmonyPatch]
    public static class cHarvestDesignatablePatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( HarvestDesignatable ), "OnClickHarvestWhenReady" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onClickHarvestWhenReady( HarvestDesignatable __instance ) => markForHarvest( true, __instance );

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( HarvestDesignatable ), "OnClickCancelHarvestWhenReady" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onClickCancelHarvestWhenReady( HarvestDesignatable __instance ) => markForHarvest( false, __instance );

        private static void markForHarvest( bool _marked, HarvestDesignatable _instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            KPrefabID prefab_id = _instance.GetComponent< KPrefabID >();
            if( prefab_id == null )
                return;

            cAutoHarvestPacket packet = new cAutoHarvestPacket( _marked, prefab_id.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}