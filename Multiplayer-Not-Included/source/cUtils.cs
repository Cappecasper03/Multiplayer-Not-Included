using System.Threading.Tasks;
using HarmonyLib;
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

        public static T    getField< T >( object _object, string _field_name )           => Traverse.Create( _object ).Field( _field_name ).GetValue< T >();
        public static void setField< T >( object _object, string _field_name, T _value ) => Traverse.Create( _object ).Field( _field_name ).SetValue( _value );

        public static void invokeMethod( object _object, string _method_name ) => Traverse.Create( _object ).Method( _method_name ).GetValue();

        public static void invokeMethod( object _object, string _method_name, params object[] _parameters )
        {
            Traverse.Create( _object ).Method( _method_name, _parameters ).GetValue( _parameters );
        }
    }
}