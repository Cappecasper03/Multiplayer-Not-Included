using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;

namespace MultiplayerNotIncluded.Patches.Tool
{
    [HarmonyPatch]
    public static class cDeconstructToolPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( DeconstructTool ), nameof( DeconstructTool.DeconstructCell ) )]
        private static void deconstructCell( int cell )
        {
            if( !cSteamLobby.inLobby() || s_skip_sending )
                return;

            cDeconstructToolPacket packet = new cDeconstructToolPacket( cell );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}