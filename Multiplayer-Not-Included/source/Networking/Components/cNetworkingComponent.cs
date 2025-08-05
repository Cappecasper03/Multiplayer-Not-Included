using MultiplayerNotIncluded.Networking;
using UnityEngine;

namespace MultiplayerNotIncluded.source.Networking.Components
{
    public class cNetworkingComponent : MonoBehaviour
    {
        private void Update()
        {
            if( !SteamManager.Initialized )
                return;

            if( !cMultiplayerSession.s_in_session )
                return;

            if( cMultiplayerSession.isHost )
                cGameServer.update();
            else if( cMultiplayerSession.isClient && cMultiplayerSession.m_host_steam_id.IsValid() )
                cGameClient.update();
        }

        private void OnApplicationQuit()
        {
            cSteamLobby.leave();
        }
    }
}