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
    public static class cBuildingEnabledButtonPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( BuildingEnabledButton ), "OnMenuToggle" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onMenuToggle( BuildingEnabledButton __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_sending )
                return;

            bool queued_toggle = Traverse.Create( __instance ).Field( "queuedToggle" ).GetValue< bool >();

            cBuildingEnabledPacket packet;
            cNetworkIdentity       identity = __instance.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( __instance.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                    return;

                packet = new cBuildingEnabledPacket( queued_toggle, cell, layer );
            }
            else
                packet = new cBuildingEnabledPacket( queued_toggle, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}