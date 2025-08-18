using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cDropAllWorkablePatch
    {
        public static bool s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( DropAllWorkable ), nameof( DropAllWorkable.DropAll ) )]
        private static void onFilterChanged( DropAllWorkable __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            int cell = Grid.PosToCell( __instance.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                return;

            bool marked_for_drop = Traverse.Create( __instance ).Field( "markedForDrop" ).GetValue< bool >();

            cDropAllPacket packet = new cDropAllPacket( marked_for_drop, cell, layer );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}