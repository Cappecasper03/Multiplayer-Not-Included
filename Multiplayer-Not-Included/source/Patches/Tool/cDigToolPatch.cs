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
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( DigTool ), nameof( DigTool.PlaceDig ) )]
        private static void placeDig( int cell, int animationDelay, GameObject __result )
        {
            if( !cSteamLobby.inLobby() || __result == null )
                return;

            cDigToolPacket packet = new cDigToolPacket( cell, animationDelay );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}