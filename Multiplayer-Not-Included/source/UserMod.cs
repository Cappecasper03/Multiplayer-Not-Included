using HarmonyLib;
using KMod;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Networking;

namespace MultiplayerNotIncluded
{
    public class UserMod : UserMod2
    {
        public override void OnLoad( Harmony harmony )
        {
            base.OnLoad( harmony );

            DebugMenu.Initialize();
            SteamLobby.Initialize();

            Debug.Log( "[UserMod] Oni multiplayer mod loaded" );
        }
    }
}