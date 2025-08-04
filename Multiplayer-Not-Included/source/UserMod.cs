using HarmonyLib;
using KMod;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.source.Networking.Components;
using UnityEngine;

namespace MultiplayerNotIncluded
{
    public class UserMod : UserMod2
    {
        public override void OnLoad( Harmony harmony )
        {
            base.OnLoad( harmony );

            DebugMenu.Initialize();
            SteamLobby.Initialize();
            PacketConstructorRegistry.Initialize();

            GameObject gameObject = new GameObject( "MNI_Components" );
            Object.DontDestroyOnLoad( gameObject );
            gameObject.AddComponent< NetworkingComponent >();

            DebugTools.Logger.LogInfo( "Mod successfully loaded" );
        }
    }
}