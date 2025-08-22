using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;
using MultiplayerNotIncluded.source.Networking.Components;

namespace MultiplayerNotIncluded.source.Patches.Minions
{
    [HarmonyPatch]
    public static class cMinionIdentityPatch
    {
        public static bool s_skip_send = false;

        [HarmonyPrefix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( MinionIdentity ), nameof( MinionIdentity.SetName ) )]
        private static void setName( string name, MinionIdentity __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            cNetworkIdentity identity = __instance.GetComponent< cNetworkIdentity >();
            if( identity == null || identity.getNetworkId() < 0 )
                return;

            cRenamePacket packet = new cRenamePacket( identity.getNetworkId(), name );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}