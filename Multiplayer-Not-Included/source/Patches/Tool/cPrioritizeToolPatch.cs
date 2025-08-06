using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;
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
        private static void tryPrioritizeGameObject( GameObject target, PrioritySetting priority )
        {
            if( !cSteamLobby.inLobby() || s_skip_sending )
                return;

            KPrefabID prefab_id = target?.GetComponent< KPrefabID >();
            if( prefab_id == null )
                return;

            cPrioritizeToolPacket packet = new cPrioritizeToolPacket( prefab_id.InstanceID, priority );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}