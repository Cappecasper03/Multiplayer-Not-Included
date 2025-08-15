using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cDirectionControlPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( DirectionControl ), "OnChangeWorkableDirection" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onChangeWorkableDirection( DirectionControl __instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            int cell = Grid.PosToCell( __instance.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                return;

            cDirectionControlPacket packet = new cDirectionControlPacket( __instance.allowedDirection, cell, layer );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}