using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cDeconstructablePatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Deconstructable ), "OnDeconstruct" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onDeconstruct( Deconstructable __instance )
        {
            if( !cSession.inSession() )
                return;

            KPrefabID prefab_id = __instance.GetComponent< KPrefabID >();
            if( prefab_id == null )
                return;

            cDeconstructPacket packet = new cDeconstructPacket( !__instance.IsMarkedForDeconstruction(), prefab_id.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}