using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Items;

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

            KPrefabID prefab_id = _instance.GetComponent< KPrefabID >();
            if( prefab_id == null )
                return;

            cClearItemPacket packet = new cClearItemPacket( _marked, prefab_id.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}