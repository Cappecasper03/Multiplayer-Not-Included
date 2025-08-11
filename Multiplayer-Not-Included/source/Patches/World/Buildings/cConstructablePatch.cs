using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cConstructablePatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Constructable ), "OnPressCancel" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onPressCancel( Constructable __instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            cCancelToolPacket packet = cCancelToolPacket.createCell( Grid.PosToCell( __instance.transform.localPosition ) );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}