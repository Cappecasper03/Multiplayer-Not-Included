using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.Tool
{
    [HarmonyPatch]
    public static class cCancelToolPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( CancelTool ), "OnDragTool" )]
        [HarmonyPatch( new[] { typeof( int ), typeof( int ) } )]
        private static void onDragTool( int cell, int distFromOrigin )
        {
            if( !cSession.inSessionAndReady() || s_skip_sending )
                return;

            cCancelToolPacket packet = cCancelToolPacket.createCell( cell );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( CancelTool ), "OnDragComplete" )]
        [HarmonyPatch( new[] { typeof( Vector3 ), typeof( Vector3 ) } )]
        private static void onDragComplete( Vector3 downPos, Vector3 upPos, CancelTool __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_sending )
                return;

            cCancelToolPacket packet = cCancelToolPacket.createArea( downPos, upPos );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}