using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;
using MultiplayerNotIncluded.source.Networking.Components;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cDeconstructablePatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Deconstructable ), "OnDeconstruct" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onDeconstruct( Deconstructable __instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            cDeconstructPacket packet;
            cNetworkIdentity   identity = __instance.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( __instance.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                    return;

                packet = new cDeconstructPacket( !__instance.IsMarkedForDeconstruction(), cell, layer );
            }
            else
                packet = new cDeconstructPacket( !__instance.IsMarkedForDeconstruction(), identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}