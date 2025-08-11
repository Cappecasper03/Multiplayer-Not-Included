using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Plants;

namespace MultiplayerNotIncluded.Patches.World.Items
{
    [HarmonyPatch]
    public static class cUprootablePatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Uprootable ), "OnClickUproot" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onClickUproot( Uprootable __instance ) => markForUproot( true, __instance );

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Uprootable ), "OnClickCancelUproot" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onClickCancelUproot( Uprootable __instance ) => markForUproot( false, __instance );

        private static void markForUproot( bool _marked, Uprootable _instance )
        {
            cLogger.logWarning( "Marking carve" );
            if( !cSession.inSessionAndReady() )
                return;

            KPrefabID prefab_id = _instance.GetComponent< KPrefabID >();
            if( prefab_id == null )
                return;

            cUprootPlantPacket packet = new cUprootPlantPacket( _marked, prefab_id.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}