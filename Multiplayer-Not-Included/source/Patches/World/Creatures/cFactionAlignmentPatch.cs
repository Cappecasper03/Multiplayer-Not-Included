using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Creatures;

namespace MultiplayerNotIncluded.Patches.World.Creatures
{
    [HarmonyPatch]
    public static class cFactionAlignmentPatch
    {
        public static bool s_skip_send = true;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( FactionAlignment ), nameof( FactionAlignment.SetPlayerTargeted ) )]
        private static void setPlayerTargeted( bool state, FactionAlignment __instance )
        {
            if( !cSession.inSession() || s_skip_send )
                return;

            cAttackCreaturePacket packet = new cAttackCreaturePacket( state, __instance.kprefabID.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}