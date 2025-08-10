using System;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches
{
    [HarmonyPatch]
    public static class cMainMenuPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( MainMenu ), "OnPrefabInit" )]
        private static void onPrefabInit( MainMenu __instance )
        {
            ColorStyleSetting style            = Traverse.Create( __instance ).Field( "normalButtonStyle" ).GetValue< ColorStyleSetting >();
            var               button_info_type = AccessTools.Inner( typeof( MainMenu ), "ButtonInfo" );

            var new_button_info = Activator.CreateInstance(
                button_info_type,
                new LocString( "Join Game" ),
                ( System.Action )( () => { SteamFriends.ActivateGameOverlay( "friends" ); } ),
                22,
                style );

            Traverse.Create( __instance ).Method( "MakeButton", new_button_info ).GetValue();

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