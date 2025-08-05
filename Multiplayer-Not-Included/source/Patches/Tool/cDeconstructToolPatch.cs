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
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( DeconstructTool ), "DeconstructCell" )]
        [HarmonyPatch( new[] { typeof( int ) } )]
        private static void deconstructCell( int cell )
        {
            if( !cSteamLobby.inLobby() )
                return;

            cDeconstructToolPacket packet = new cDeconstructToolPacket( cell );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}