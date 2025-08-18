using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cPlayerControlledToggleSideScreenPatch
    {
        public static bool s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( PlayerControlledToggleSideScreen ), "ClickToggle" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void clickToggle( PlayerControlledToggleSideScreen __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            KMonoBehaviour mono_behaviour = __instance.target as KMonoBehaviour;
            if( mono_behaviour == null )
                return;

            bool value = __instance.target.ToggledOn();
            if( SpeedControlScreen.Instance.IsPaused )
                value = __instance.target.ToggleRequested;

            int cell = Grid.PosToCell( mono_behaviour.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, mono_behaviour.gameObject, out layer ) )
                return;

            cTogglePacket packet = new cTogglePacket( value, cell, layer );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}