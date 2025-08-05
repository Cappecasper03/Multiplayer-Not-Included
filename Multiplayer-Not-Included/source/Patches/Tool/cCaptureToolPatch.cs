using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.Tool
{
    [HarmonyPatch]
    public static class cCaptureToolPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( CaptureTool ), "OnDragComplete" )]
        [HarmonyPatch( new[] { typeof( Vector3 ), typeof( Vector3 ) } )]
        private static void onDragComplete( Vector3 downPos, Vector3 upPos )
        {
            if( !cSteamLobby.inLobby() )
                return;

            Vector2 min = cUtils.getRegularizedPos( Vector2.Min( downPos, upPos ), true );
            Vector2 max = cUtils.getRegularizedPos( Vector2.Max( downPos, upPos ), false );

            cCaptureToolPacket packet = new cCaptureToolPacket( min, max );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}