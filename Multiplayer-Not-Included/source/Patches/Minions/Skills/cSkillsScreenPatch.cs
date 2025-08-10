using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;

namespace MultiplayerNotIncluded.source.Patches.Minions.Skills
{
    [HarmonyPatch]
    public static class cSkillsScreenPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( SkillsScreen ), "OnHatDropEntryClick" )]
        [HarmonyPatch( new[] { typeof( IListableOption ), typeof( object ) } )]
        private static void onHatDropEntryClick( IListableOption skill, object data, SkillsScreen __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_sending )
                return;

            HatListable         hat_listable = skill as HatListable;
            IAssignableIdentity identity     = Traverse.Create( __instance ).Field( "currentlySelectedMinion" ).GetValue< IAssignableIdentity >();
            if( hat_listable == null || identity == null )
                return;

            cSkillsPacket packet = cSkillsPacket.createHat( identity.GetProperName(), hat_listable.name, hat_listable.hat );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}