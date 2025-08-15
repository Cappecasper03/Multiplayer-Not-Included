using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World;

namespace MultiplayerNotIncluded.Patches.World
{
    [HarmonyPatch]
    public static class cMeterScreenPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( MeterScreen ), "OnRedAlertClick" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onRedAlertClick()
        {
            if( !cSession.inSessionAndReady() )
                return;

            cRedAlertPacket packet = new cRedAlertPacket( ClusterManager.Instance.activeWorld.AlertManager.IsRedAlertToggledOn() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}