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

            DebugTools.Logger.LogInfo( "Mod successfully loaded" );
        }
    }
}