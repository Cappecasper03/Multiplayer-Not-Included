using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;

namespace MultiplayerNotIncluded.source.Patches.Menus
{
    [HarmonyPatch]
    public static class cScheduleMinionWidgetPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ScheduleMinionWidget ), nameof( ScheduleMinionWidget.ChangeAssignment ) )]
        private static void changeAssignment( Schedule targetSchedule, Schedulable schedulable, ScheduleMinionWidget __instance )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            string          name   = __instance.schedulable != null ? __instance.schedulable.name : "";
            cSchedulePacket packet = cSchedulePacket.createChangeAssignment( name, targetSchedule.name, schedulable.name );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}