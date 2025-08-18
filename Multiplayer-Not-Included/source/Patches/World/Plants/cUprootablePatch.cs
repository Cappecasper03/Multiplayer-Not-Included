using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Plants;
using MultiplayerNotIncluded.source.Networking.Components;

namespace MultiplayerNotIncluded.Patches.World.Plants
{
    [HarmonyPatch]
    public static class cUprootablePatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Uprootable ), "OnClickUproot" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onClickUproot( Uprootable __instance ) => markForUproot( true, __instance );

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Uprootable ), "OnClickCancelUproot" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onClickCancelUproot( Uprootable __instance ) => markForUproot( false, __instance );

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Uprootable ), nameof( Uprootable.ForceCancelUproot ) )]
        private static void forceCancelUproot( object data, Uprootable __instance ) => markForUproot( false, __instance );

        private static void markForUproot( bool _marked, Uprootable _instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            cUprootPacket    packet;
            cNetworkIdentity identity = _instance.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( _instance.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, _instance.gameObject, out layer ) )
                    return;

                packet = new cUprootPacket( _marked, cell, layer );
            }
            else
                packet = new cUprootPacket( _marked, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}