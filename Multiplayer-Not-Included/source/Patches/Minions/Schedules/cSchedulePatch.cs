using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;

namespace MultiplayerNotIncluded.source.Patches.Minions.Schedules
{
    [HarmonyPatch]
    public static class cSchedulePatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Schedule ), nameof( Schedule.SetBlockGroup ) )]
        private static void setBlockGroup( int idx, ScheduleGroup group, Schedule __instance )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            cSchedulePacket packet = cSchedulePacket.createChangeSchedule( __instance.name, idx, group.Name, group.Id );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Schedule ), nameof( Schedule.ShiftTimetable ) )]
        private static void shiftTimetable( bool up, int timetableToShiftIdx, Schedule __instance )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            cSchedulePacket packet = cSchedulePacket.createShiftTimetable( __instance.name, timetableToShiftIdx, up );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Schedule ), nameof( Schedule.RotateBlocks ) )]
        private static void rotateBlocks( bool directionLeft, int timetableToRotateIdx, Schedule __instance )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            cSchedulePacket packet = cSchedulePacket.createRotateTimetable( __instance.name, timetableToRotateIdx, directionLeft );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}