using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;

namespace MultiplayerNotIncluded.source.Patches.Minions.Skills
{
    [HarmonyPatch]
    public static class cMinionResumePatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( MinionResume ), nameof( MinionResume.MasterSkill ) )]
        private static void masterSkill( string skillId, MinionResume __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_sending )
                return;

            cSkillsPacket packet = cSkillsPacket.createSkill( __instance.GetIdentity.GetProperName(), skillId );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}