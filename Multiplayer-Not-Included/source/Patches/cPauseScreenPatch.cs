using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MultiplayerNotIncluded.Networking;

namespace MultiplayerNotIncluded.Patches
{
    [HarmonyPatch( typeof( PauseScreen ), "ConfigureButtonInfos" )]
    internal static class cPauseScreenPatch
    {
        private static void postfix( PauseScreen __instance )
        {
            if( cMultiplayerSession.isClient )
                return;

            var        buttons_field   = AccessTools.Field( typeof( KModalButtonMenu ), "buttons" );
            var        button_infos    = ( ( KButtonMenu.ButtonInfo[] )buttons_field.GetValue( __instance ) )?.ToList() ?? new List< KButtonMenu.ButtonInfo >();
            MethodInfo refresh_buttons = __instance.GetType().GetMethod( "RefreshButtons", BindingFlags.Public | BindingFlags.Instance );

            if( button_infos.Any( _b => _b.text == "Start Multiplayer" ) || button_infos.Any( _b => _b.text == "Stop Multiplayer" ) )
                return;

            int index = button_infos.FindIndex( _b => _b.text == "Resume" ) + 1;
            if( index <= 0 )
                index = 1;

            KButtonMenu.ButtonInfo button = null;
            button = new KButtonMenu.ButtonInfo( cGameServer.m_state <= 0 ? "Start Server" : "Stop Server", () =>
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

                refresh_buttons?.Invoke( __instance, new object[] { } );
            } );

            button_infos.Insert( index, button );
            buttons_field.SetValue( __instance, button_infos.ToArray() );
        }
    }
}