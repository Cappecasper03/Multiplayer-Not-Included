using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Items;
using MultiplayerNotIncluded.source.Networking.Components;

namespace MultiplayerNotIncluded.Patches.World.Items
{
    [HarmonyPatch]
    public static class cClearablePatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Clearable ), "OnClickClear" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onClickClear( Clearable __instance ) => markForClear( true, __instance );

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Clearable ), "OnClickCancel" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onClickCancel( Clearable __instance ) => markForClear( false, __instance );

        private static void markForClear( bool _marked, Clearable _instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            cClearItemPacket packet;
            cNetworkIdentity identity = _instance.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( _instance.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, _instance.gameObject, out layer ) )
                    return;

                packet = new cClearItemPacket( _marked, cell, layer );
            }
            else
                packet = new cClearItemPacket( _marked, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}