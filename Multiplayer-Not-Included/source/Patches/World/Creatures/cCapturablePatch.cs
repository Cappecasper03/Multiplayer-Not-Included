using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Creatures;

namespace MultiplayerNotIncluded.Patches.World.Creatures
{
    [HarmonyPatch]
    public static class cCapturablePatch
    {
        public static bool s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Capturable ), nameof( Capturable.MarkForCapture ) )]
        [HarmonyPatch( new[] { typeof( bool ) } )]
        private static void setPlayerTargeted( bool mark, Capturable __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            KPrefabID prefab_id = __instance.GetComponent< KPrefabID >();
            if( prefab_id == null )
                return;

            cCaptureCreaturePacket packet = new cCaptureCreaturePacket( mark, prefab_id.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}