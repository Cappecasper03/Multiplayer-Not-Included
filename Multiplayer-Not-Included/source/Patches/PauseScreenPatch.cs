using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MultiplayerNotIncluded.Networking;

namespace MultiplayerNotIncluded.Patches
{
    [HarmonyPatch( typeof( PauseScreen ), "ConfigureButtonInfos" )]
    internal static class PauseScreenPatch
    {
        private static void Postfix( PauseScreen __instance )
        {
            if( MultiplayerSession.IsClient )
                return;

            var        buttonsField   = AccessTools.Field( typeof( KModalButtonMenu ), "buttons" );
            var        buttonInfos    = ( ( KButtonMenu.ButtonInfo[] )buttonsField.GetValue( __instance ) )?.ToList() ?? new List< KButtonMenu.ButtonInfo >();
            MethodInfo refreshButtons = __instance.GetType().GetMethod( "RefreshButtons", BindingFlags.Public | BindingFlags.Instance );

            if( buttonInfos.Any( b => b.text == "Start Multiplayer" ) || buttonInfos.Any( b => b.text == "Stop Multiplayer" ) )
                return;

            int index = buttonInfos.FindIndex( b => b.text == "Resume" ) + 1;
            if( index <= 0 )
                index = 1;

            KButtonMenu.ButtonInfo button = null;
            button = new KButtonMenu.ButtonInfo( GameServer.State <= 0 ? "Start Server" : "Stop Server", () =>
            {
                SteamLobby.Create();
                button.text = GameServer.State <= 0 ? "Start Server" : "Stop Server";
                refreshButtons.Invoke( __instance, new object[] { } );
            } );

            buttonInfos.Insert( index, button );
            buttonsField.SetValue( __instance, buttonInfos.ToArray() );
        }
    }
}