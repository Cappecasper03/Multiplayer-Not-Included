using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches
{
    [HarmonyPatch( typeof( MainMenu ), "OnPrefabInit" )]
    internal static class cMainMenuPatch
    {
        private static void postfix( MainMenu __instance )
        {
            const int         font_size        = 22;
            ColorStyleSetting style            = Traverse.Create( __instance ).Field( "normalButtonStyle" ).GetValue< ColorStyleSetting >();
            Type              button_info_type = __instance.GetType().GetNestedType( "ButtonInfo", BindingFlags.NonPublic );
            MethodInfo        make_button      = __instance.GetType().GetMethod( "MakeButton", BindingFlags.NonPublic | BindingFlags.Instance );

            object join_info = Activator.CreateInstance( button_info_type );
            button_info_type.GetField( "text" ).SetValue( join_info, new LocString( "Join Game" ) );
            button_info_type.GetField( "action" ).SetValue( join_info, new System.Action( () => { SteamFriends.ActivateGameOverlay( "friends" ); } ) );
            button_info_type.GetField( "fontSize" ).SetValue( join_info, font_size );
            button_info_type.GetField( "style" ).SetValue( join_info, style );
            make_button?.Invoke( __instance, new object[] { join_info } );

            updatePlacements( __instance );
        }

        private static void updatePlacements( MainMenu __instance )
        {
            GameObject button_parent = Traverse.Create( __instance ).Field( "buttonParent" ).GetValue< GameObject >();
            if( !button_parent )
                return;

            KButton[] children = button_parent.GetComponentsInChildren< KButton >();

            KButton load_game_button = children.FirstOrDefault( _b => _b.GetComponentInChildren< LocText >().text.ToLower().Contains( "load game" ) );

            KButton join_game_button = children.FirstOrDefault( _b => _b.GetComponentInChildren< LocText >().text.ToLower().Contains( "join game" ) );

            if( !load_game_button || !join_game_button )
                return;

            int load_index = load_game_button.transform.GetSiblingIndex();

            join_game_button.transform.SetSiblingIndex( load_index + 1 );
        }
    }
}