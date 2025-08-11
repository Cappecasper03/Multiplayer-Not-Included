using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerNotIncluded
{
    public static class cUtils
    {
        private static Dictionary< string, MinionIdentity > s_identity = new Dictionary< string, MinionIdentity >();

        public static bool isInMenu() => App.GetCurrentSceneName() == "frontend";
        public static bool isInGame() => App.GetCurrentSceneName() == "backend";

        public static async void delayAction( int _delay, UnityAction _action )
        {
            if( _action == null )
                return;

            await Task.Delay( _delay );
            _action.Invoke();
        }

        public static bool findAndCache( string _name, out MinionIdentity _identity )
        {
            if( s_identity.TryGetValue( _name, out _identity ) )
                return true;

            MinionIdentity[] minion_identities = Object.FindObjectsOfType< MinionIdentity >();
            foreach( MinionIdentity identity in minion_identities )
            {
                if( identity.GetProperName() != _name )
                    continue;

                s_identity.Add( _name, identity );
                _identity = identity;
                return true;
            }

            return false;
        }
    }
}