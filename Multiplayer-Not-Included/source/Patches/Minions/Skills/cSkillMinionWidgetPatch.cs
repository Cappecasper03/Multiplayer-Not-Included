using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;
using MultiplayerNotIncluded.source.Networking.Components;
using UnityEngine;

namespace MultiplayerNotIncluded.source.Patches.Minions.Skills
{
    [HarmonyPatch]
    public static class cSkillMinionWidgetPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( SkillMinionWidget ), "OnHatDropEntryClick" )]
        [HarmonyPatch( new[] { typeof( IListableOption ), typeof( object ) } )]
        private static void onHatDropEntryClick( IListableOption hatOption, object data, SkillMinionWidget __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_sending )
                return;

            SkillsScreen skill_screen = Object.FindObjectOfType< SkillsScreen >();
            if( skill_screen == null )
                return;

            HatListable    hat_listable = hatOption as HatListable;
            MinionIdentity minion_identity;
            skill_screen.GetMinionIdentity( __instance.assignableIdentity, out minion_identity, out var stored_minion );
            cNetworkIdentity identity = minion_identity?.GetComponent< cNetworkIdentity >();
            if( hat_listable == null || identity == null )
                return;

            cSkillsPacket packet = cSkillsPacket.createHat( identity.getNetworkId(), hat_listable.name, hat_listable.hat );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}