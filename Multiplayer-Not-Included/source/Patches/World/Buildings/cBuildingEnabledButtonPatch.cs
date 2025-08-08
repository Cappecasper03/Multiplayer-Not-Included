using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

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
            if( !cSession.inSession() || s_skip_sending )
                return;

            KPrefabID prefab_id = __instance.GetComponent< KPrefabID >();
            if( prefab_id == null )
                return;

            bool queued_toggle = ( bool )AccessTools.Field( typeof( BuildingEnabledButton ), "queuedToggle" ).GetValue( __instance );

            cBuildingEnabledPacket packet = new cBuildingEnabledPacket( queued_toggle, prefab_id.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}