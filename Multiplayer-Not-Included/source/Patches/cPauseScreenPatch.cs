using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;

namespace MultiplayerNotIncluded.Patches
{
    [HarmonyPatch]
    public static class cPauseScreenPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( PauseScreen ), "ConfigureButtonInfos" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void configureButtonInfos( PauseScreen __instance )
        {
            if( cSession.isClient() )
                return;

            List< KButtonMenu.ButtonInfo > button_infos    = Traverse.Create( __instance ).Field( "buttons" ).GetValue< KButtonMenu.ButtonInfo[] >()?.ToList();
            Traverse                       refresh_buttons = Traverse.Create( __instance ).Method( "RefreshButtons" );

            if( button_infos == null || refresh_buttons == null )
                return;

            if( button_infos.Any( _b => _b.text == "Start Multiplayer" ) || button_infos.Any( _b => _b.text == "Stop Multiplayer" ) )
                return;

            int index = button_infos.FindIndex( _b => _b.text == "Resume" ) + 1;
            if( index <= 0 )
                index = 1;

            KButtonMenu.ButtonInfo button = null;
            button = new KButtonMenu.ButtonInfo( cServer.m_state <= 0 ? "Start Server" : "Stop Server", () =>
            {
                if( button.text == "Start Server" )
                {
                    cSteamLobby.create();
                    button.text = "Stop Server";
                }
                else
                {
                    cSteamLobby.leave();
                    button.text = "Start Server";
                }

                refresh_buttons?.GetValue();
            } );

            button_infos.Insert( index, button );
            Traverse.Create( __instance ).Field( "buttons" ).SetValue( button_infos.ToArray() );
        }

        [HarmonyPrefix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( PauseScreen ), "OnQuitConfirm" )]
        [HarmonyPatch( new[] { typeof( bool ) } )]
        private static void onQuitConfirm( bool saveFirst )
        {
            if( !cSteamLobby.inLobby() )
                return;

            cSteamLobby.leave();
        }
    }
}