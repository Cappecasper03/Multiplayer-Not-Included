using HarmonyLib;
using KMod;
using OniMultiplayerMod.Networking;

namespace OniMultiplayerMod
{
    public class UserMod : UserMod2
    {
        public override void OnLoad( Harmony harmony )
        {
            base.OnLoad( harmony );

            SteamLobby.Initialize();

            Debug.Log( "[UserMod] Oni multiplayer mod loaded" );
        }
    }
}