using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.Tool
{
    [HarmonyPatch]
    public static class cDigToolPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( DigTool ), nameof( DigTool.PlaceDig ) )]
        private static void placeDig( int cell, int animationDelay, GameObject __result )
        {
            if( !cSession.inSessionAndReady() || __result == null || s_skip_sending )
                return;

            cDigToolPacket packet = new cDigToolPacket( cell, animationDelay );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}