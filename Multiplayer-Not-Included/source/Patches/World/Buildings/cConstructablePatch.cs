using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cConstructablePatch
    {
        public static bool s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Constructable ), "OnPressCancel" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onPressCancel( Constructable __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            int cell = Grid.PosToCell( __instance.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                return;

            cCancelBuildPacket packet = new cCancelBuildPacket( cell, layer );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}