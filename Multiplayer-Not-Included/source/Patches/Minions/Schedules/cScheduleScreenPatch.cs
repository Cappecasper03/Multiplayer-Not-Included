using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;

namespace MultiplayerNotIncluded.source.Patches.Minions.Schedules
{
    [HarmonyPatch]
    public static class cScheduleScreenPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ScheduleScreen ), "OnAddScheduleClick" )]
        private static void onAddScheduleClick()
        {
            if( !cSession.inSessionAndReady() || s_skip_sending )
                return;

            cSchedulePacket packet = cSchedulePacket.createDefault();

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}