using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Menus;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Player;
using Steamworks;

namespace MultiplayerNotIncluded.Patches
{
    [HarmonyPatch]
    public static class cSaveLoaderPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( SaveLoader ), "OnSpawn" )]
        private static void OnSpawn()
        {
            if( !cSteamLobby.inLobby() )
                return;

            cMultiplayerLoadingOverlay.show( $"Waiting for {SteamFriends.GetFriendPersonaName( cSession.m_host_steam_id )}..." );
            SpeedControlScreen.Instance.Pause( false );

            cPacketSender.sendToHost( new cPlayerReadyPacket( cSession.m_local_steam_id ) );
        }
    }
}