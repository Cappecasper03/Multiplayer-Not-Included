using System.Threading.Tasks;
using UnityEngine;
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

        public static bool tryGetLayer( int _cell, GameObject _object, out int _layer )
        {
            for( int i = 0; i < Grid.ObjectLayers.Length; i++ )
            {
                if( Grid.Objects[ _cell, i ] != _object )
                    continue;

                _layer = i;
                return true;
            }

            _layer = 0;
            return false;
        }
    }
}