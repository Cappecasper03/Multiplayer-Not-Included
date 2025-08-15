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
    public static class cRepairablePatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Repairable ), "AllowRepair" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void allowRepair( Repairable __instance ) => changeRepair( true, __instance );

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Repairable ), nameof( Repairable.CancelRepair ) )]
        private static void cancelRepair( Repairable __instance ) => changeRepair( false, __instance );

        private static void changeRepair( bool _allow, Repairable _instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_sending )
                return;

            cAutoRepairPacket packet;
            cNetworkIdentity  identity = _instance.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( _instance.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, _instance.gameObject, out layer ) )
                    return;

                packet = new cAutoRepairPacket( _allow, cell, layer );
            }
            else
                packet = new cAutoRepairPacket( _allow, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}