using System.Threading.Tasks;
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
    }
}