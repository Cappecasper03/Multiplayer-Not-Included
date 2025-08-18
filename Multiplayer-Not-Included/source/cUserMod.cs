using HarmonyLib;
using KMod;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.source.Networking.Components;
using UnityEngine;

namespace MultiplayerNotIncluded
{
    public class cUserMod : UserMod2
    {
        public override void OnLoad( Harmony _harmony )
        {
            base.OnLoad( _harmony );

            cDebugMenu.initialize();
            cSteamLobby.initialize();
            cPacketFactory.initialize();

            GameObject game_object = new GameObject( "MultiplayerNotIncluded" );
            Object.DontDestroyOnLoad( game_object );
            game_object.AddComponent< cNetworkingComponent >();
            game_object.AddComponent< cPlayerCursorComponent >().initialize( cSession.m_local_steam_id );

            cLogger.logInfo( "Mod successfully loaded" );
        }
    }
}