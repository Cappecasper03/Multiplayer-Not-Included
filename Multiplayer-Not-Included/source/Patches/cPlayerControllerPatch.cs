using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Menus;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Players;
using Steamworks;

namespace MultiplayerNotIncluded.Patches.World
{
    [HarmonyPatch]
    public static class cPlayerControllerPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( PlayerController ), "OnSpawn" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onSpawn()
        {
            if( !cSteamLobby.inLobby() )
                return;

            cMultiplayerLoadingOverlay.show( $"Waiting for {SteamFriends.GetFriendPersonaName( cSession.m_host_steam_id )}..." );
            if( !SpeedControlScreen.Instance.IsPaused )
                SpeedControlScreen.Instance.Pause( false );

            cPacketSender.sendToHost( new cPlayerReadyPacket() );
        }
    }
}