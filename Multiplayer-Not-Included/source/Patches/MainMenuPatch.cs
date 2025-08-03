using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MultiplayerNotIncluded.Networking;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches
{
    [HarmonyPatch( typeof( MainMenu ), "OnPrefabInit" )]
    internal static class MainMenuPatch
    {
        private static void Postfix( MainMenu __instance )
        {
            const int         fontSize       = 22;
            ColorStyleSetting style          = Traverse.Create( __instance ).Field( "normalButtonStyle" ).GetValue< ColorStyleSetting >();
            Type              buttonInfoType = __instance.GetType().GetNestedType( "ButtonInfo", BindingFlags.NonPublic );
            MethodInfo        makeButton     = __instance.GetType().GetMethod( "MakeButton", BindingFlags.NonPublic | BindingFlags.Instance );

            object hostInfo = Activator.CreateInstance( buttonInfoType );
            buttonInfoType.GetField( "text" ).SetValue( hostInfo, new LocString( "Host Game" ) );
            buttonInfoType.GetField( "action" ).SetValue( hostInfo, new System.Action( () =>
            {
                MultiplayerSession.ShouldHostAfterLoad = true;
                __instance.Button_ResumeGame.SignalClick( KKeyCode.Mouse0 );
            } ) );
            buttonInfoType.GetField( "fontSize" ).SetValue( hostInfo, fontSize );
            buttonInfoType.GetField( "style" ).SetValue( hostInfo, style );
            makeButton.Invoke( __instance, new object[] { hostInfo } );

            object joinInfo = Activator.CreateInstance( buttonInfoType );
            buttonInfoType.GetField( "text" ).SetValue( joinInfo, new LocString( "Join Game" ) );
            buttonInfoType.GetField( "action" ).SetValue( joinInfo, new System.Action( () => { SteamFriends.ActivateGameOverlay( "friends" ); } ) );
            buttonInfoType.GetField( "fontSize" ).SetValue( joinInfo, fontSize );
            buttonInfoType.GetField( "style" ).SetValue( joinInfo, style );
            makeButton.Invoke( __instance, new object[] { joinInfo } );

            UpdatePlacements( __instance );
        }

        private static void UpdatePlacements( MainMenu __instance )
        {
            GameObject buttonParent = Traverse.Create( __instance ).Field( "buttonParent" ).GetValue< GameObject >();
            if( !buttonParent )
                return;

            KButton[] children = buttonParent.GetComponentsInChildren< KButton >();

            KButton loadGameButton = children.FirstOrDefault( b => b.GetComponentInChildren< LocText >().text.ToLower().Contains( "load game" ) );

            KButton hostGameButton = children.FirstOrDefault( b => b.GetComponentInChildren< LocText >().text.ToLower().Contains( "host game" ) );
            KButton joinGameButton = children.FirstOrDefault( b => b.GetComponentInChildren< LocText >().text.ToLower().Contains( "join game" ) );

            if( !loadGameButton || !hostGameButton || !joinGameButton )
                return;

            int loadIndex = loadGameButton.transform.GetSiblingIndex();

            hostGameButton.transform.SetSiblingIndex( loadIndex + 1 );
            joinGameButton.transform.SetSiblingIndex( loadIndex + 2 );
        }
    }
}