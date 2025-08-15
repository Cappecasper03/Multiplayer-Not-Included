using MultiplayerNotIncluded.Networking;
using UnityEngine;

namespace MultiplayerNotIncluded.source.Networking.Components
{
    public class cNetworkingComponent : MonoBehaviour
    {
        private void Update()
        {
            if( !SteamManager.Initialized || !cSteamLobby.inLobby() )
                return;

            if( cSession.isHost() )
                cServer.update();
            else if( cSession.isClient() && cSession.m_host_steam_id.IsValid() )
                cClient.update();
        }

        private void OnApplicationQuit() => cSteamLobby.leave();
    }
}