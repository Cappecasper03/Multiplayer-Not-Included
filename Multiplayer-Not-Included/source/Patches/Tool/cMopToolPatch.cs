using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;

namespace MultiplayerNotIncluded.Patches.Tool
{
    [HarmonyPatch]
    public static class cMopToolPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( MopTool ), "OnDragTool" )]
        [HarmonyPatch( new[] { typeof( int ), typeof( int ) } )]
        private static void onDragTool( int cell, int distFromOrigin )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            cMopToolPacket packet = new cMopToolPacket( cell );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}