using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerNotIncluded
{
    public static class cCacheManager
    {
        private static readonly Dictionary< string, MinionIdentity > s_minion_identities = new Dictionary< string, MinionIdentity >();
        private static readonly Dictionary< string, ResearchEntry >  s_research_entries  = new Dictionary< string, ResearchEntry >();

        public static void clear()
        {
            s_minion_identities.Clear();
            s_research_entries.Clear();
        }

        public static bool findAndCache( string _name, out MinionIdentity _identity )
        {
            if( s_minion_identities.TryGetValue( _name, out _identity ) )
                return true;

            MinionIdentity[] minion_identities = Object.FindObjectsOfType< MinionIdentity >();
            foreach( MinionIdentity identity in minion_identities )
            {
                if( identity.GetProperName() != _name )
                    continue;

                s_minion_identities.Add( _name, identity );
                _identity = identity;
                return true;
            }

            return false;
        }

        public static bool findAndCache( string _name, out ResearchEntry _entry )
        {
            if( s_research_entries.TryGetValue( _name, out _entry ) )
                return true;

            ResearchEntry[] research_entries = Object.FindObjectsOfType< ResearchEntry >();
            foreach( ResearchEntry research_entry in research_entries )
            {
                if( research_entry.name != _name )
                    continue;

                s_research_entries.Add( _name, research_entry );
                _entry = research_entry;
                return true;
            }

            return false;
        }
    }
}