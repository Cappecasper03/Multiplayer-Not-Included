using System;
using MultiplayerNotIncluded.Networking;
using UnityEngine;

namespace MultiplayerNotIncluded.source.Networking.Components
{
    public class NetworkingComponent : MonoBehaviour
    {
        private void Update()
        {
            if( !SteamManager.Initialized )
                return;

            if( !MultiplayerSession.InSession )
                return;

            if( MultiplayerSession.IsHost )
                GameServer.Update();
            else if( MultiplayerSession.IsClient && MultiplayerSession.HostSteamID.IsValid() )
                GameClient.Update();
        }
    }
}