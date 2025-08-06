using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

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
            if( !cSteamLobby.inLobby() || s_skip_sending )
                return;

            KPrefabID prefab_id = _instance.GetComponent< KPrefabID >();
            if( prefab_id == null )
                return;

            cAutoDisinfectPacket packet = new cAutoDisinfectPacket( _enable, prefab_id.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}