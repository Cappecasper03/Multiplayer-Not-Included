using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine.Events;

namespace MultiplayerNotIncluded
{
    public static class cUtils
    {
        public static bool isInMenu() => App.GetCurrentSceneName() == "frontend";
        public static bool isInGame() => App.GetCurrentSceneName() == "backend";

        public static async void delayAction( int _delay, UnityAction _action )
        {
            if( _action == null )
                return;

            await Task.Delay( _delay );
            _action.Invoke();
        }

        public static void initializeUtility( string _category, string _name )
        {
            var entries = Traverse.Create( PlanScreen.Instance ).Field( "toggleEntries" ).GetValue< IList >();
            var toggles = Traverse.Create( PlanScreen.Instance ).Field( "allBuildingToggles" ).GetValue< Dictionary< string, PlanBuildingToggle > >();
            if( entries == null || toggles == null )
                return;

            foreach( var entry in entries )
            {
                var info = Traverse.Create( entry ).Field( "toggleInfo" ).GetValue< KIconToggleMenu.ToggleInfo >();
                if( info.text != _category )
                    continue;

                info.toggle.Click();
                PlanBuildingToggle toggle;
                if( toggles.TryGetValue( _name, out toggle ) )
                    toggle.Click();
                else
                    return;

                info.toggle.Click();
                info.toggle.Click();
                break;
            }
        }
    }
}