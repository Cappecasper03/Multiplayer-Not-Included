using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Plants;
using MultiplayerNotIncluded.source.Networking.Components;

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

            cAutoHarvestPacket packet;
            cNetworkIdentity   identity = _instance.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( _instance.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, _instance.gameObject, out layer ) )
                    return;

                packet = new cAutoHarvestPacket( _marked, cell, layer );
            }
            else
                packet = new cAutoHarvestPacket( _marked, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}