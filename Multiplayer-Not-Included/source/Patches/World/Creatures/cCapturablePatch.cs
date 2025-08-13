using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Creatures;
using MultiplayerNotIncluded.source.Networking.Components;

namespace MultiplayerNotIncluded.Patches.World.Creatures
{
    [HarmonyPatch]
    public static class cCapturablePatch
    {
        public static bool s_skip_send = false;

        [HarmonyPrefix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Capturable ), "OnTagsChanged" )]
        [HarmonyPatch( new[] { typeof( object ) } )]
        private static void onTagsChangedPre( object data, Capturable __instance ) => s_skip_send = true;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Capturable ), "OnTagsChanged" )]
        [HarmonyPatch( new[] { typeof( object ) } )]
        private static void onTagsChangedPost( object data, Capturable __instance ) => s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Capturable ), nameof( Capturable.MarkForCapture ) )]
        [HarmonyPatch( new[] { typeof( bool ) } )]
        private static void markForCapture( bool mark, Capturable __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            cCaptureCreaturePacket packet;
            cNetworkIdentity       identity = __instance.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( __instance.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                    return;

                packet = new cCaptureCreaturePacket( mark, cell, layer );
            }
            else
                packet = new cCaptureCreaturePacket( mark, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}