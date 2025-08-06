using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

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
            if( !cSteamLobby.inLobby() || s_skip_sending )
                return;

            KPrefabID prefab_id = _instance.GetComponent< KPrefabID >();
            if( prefab_id == null )
                return;

            cAutoRepairPacket packet = new cAutoRepairPacket( _allow, prefab_id.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}