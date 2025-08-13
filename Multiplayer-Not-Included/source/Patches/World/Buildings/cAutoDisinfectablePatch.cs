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
    public static class cAutoDisinfectablePatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( AutoDisinfectable ), "EnableAutoDisinfect" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void enableAutoDisinfect( AutoDisinfectable __instance ) => changeDisinfect( true, __instance );

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( AutoDisinfectable ), "DisableAutoDisinfect" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void disableAutoDisinfect( AutoDisinfectable __instance ) => changeDisinfect( false, __instance );

        private static void changeDisinfect( bool _enable, AutoDisinfectable _instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_sending )
                return;

            cAutoDisinfectPacket packet;
            cNetworkIdentity     identity = _instance.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( _instance.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, _instance.gameObject, out layer ) )
                    return;

                packet = new cAutoDisinfectPacket( _enable, cell, layer );
            }
            else
                packet = new cAutoDisinfectPacket( _enable, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}