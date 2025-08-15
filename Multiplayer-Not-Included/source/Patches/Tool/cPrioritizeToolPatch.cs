using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using MultiplayerNotIncluded.source.Networking.Components;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.Tool
{
    [HarmonyPatch]
    public static class cPrioritizeToolPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( PrioritizeTool ), "TryPrioritizeGameObject" )]
        [HarmonyPatch( new[] { typeof( GameObject ), typeof( PrioritySetting ) } )]
        private static void tryPrioritizeGameObject( GameObject target, PrioritySetting priority )
        {
            if( !cSession.inSessionAndReady() || s_skip_sending )
                return;

            cPrioritizeToolPacket packet;
            cNetworkIdentity      identity = target.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( target.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, target, out layer ) )
                    return;

                packet = cPrioritizeToolPacket.createStatic( priority, cell, layer );
            }
            else
                packet = cPrioritizeToolPacket.createDynamic( priority, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}