using System;
using System.Collections.Generic;
using Satsuma;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded
{
    public static class cCacheManager
    {
        private static readonly Dictionary< System.Type, object > s_caches = new Dictionary< System.Type, object >();

        public static void clear()
        {
            foreach( object cache in s_caches.Values )
            {
                if( cache is IClearable clearable_cache )
                    clearable_cache.Clear();
            }
        }

        public static bool findAndCache< T >( string _name, Func< string, T, bool > _action, out T _out )
            where T : KMonoBehaviour
        {
            Dictionary< string, T > cache = getCache< T >();

            if( cache.TryGetValue( _name, out _out ) )
                return true;

            T[] type_objects = Object.FindObjectsOfType< T >();
            foreach( T type_object in type_objects )
            {
                if( !_action.Invoke( _name, type_object ) )
                    continue;

                cache.Add( _name, type_object );
                _out = type_object;
                return true;
            }

            return false;
        }

        private static Dictionary< string, T > getCache< T >()
            where T : KMonoBehaviour
        {
            if( s_caches.TryGetValue( typeof( T ), out var cache ) )
                return ( Dictionary< string, T > )cache;

            cache                   = new Dictionary< string, T >();
            s_caches[ typeof( T ) ] = cache;

            return ( Dictionary< string, T > )cache;
        }
    }
}