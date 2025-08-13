using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Creatures;
using MultiplayerNotIncluded.source.Networking.Components;

namespace MultiplayerNotIncluded.Patches.World.Creatures
{
    [HarmonyPatch]
    public static class cFactionAlignmentPatch
    {
        public static bool s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( FactionAlignment ), nameof( FactionAlignment.SetPlayerTargeted ) )]
        private static void setPlayerTargeted( bool state, FactionAlignment __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            cAttackCreaturePacket packet;
            cNetworkIdentity      identity = __instance.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( __instance.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                    return;

                packet = new cAttackCreaturePacket( state, cell, layer );
            }
            else
                packet = new cAttackCreaturePacket( state, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}