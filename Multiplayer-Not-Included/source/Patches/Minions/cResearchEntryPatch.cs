using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;

namespace MultiplayerNotIncluded.source.Patches.Minions
{
    [HarmonyPatch]
    public static class cResearchEntryPatch
    {
        public static bool s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ResearchEntry ), "OnResearchClicked" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onResearchClicked( ResearchEntry __instance ) => changeResearch( false, __instance );

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ResearchEntry ), "OnResearchCanceled" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onResearchCanceled( ResearchEntry __instance ) => changeResearch( true, __instance );

        private static void changeResearch( bool _cancel, ResearchEntry _instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            cResearchPacket packet;

            if( _cancel )
                packet = cResearchPacket.createCancel( _instance.name );
            else
                packet = cResearchPacket.createStart( _instance.name );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}