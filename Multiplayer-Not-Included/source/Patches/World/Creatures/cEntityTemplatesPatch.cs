using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.source.Networking.Components;
using UnityEngine;

namespace MultiplayerNotIncluded.source.Patches.World.Creatures
{
    [HarmonyPatch]
    public static class cEntityTemplatesPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( EntityTemplates ), nameof( EntityTemplates.AddCreatureBrain ) )]
        private static void createPrefab( GameObject prefab, ChoreTable.Builder chore_table, Tag species, string symbol_prefix )
        {
            SaveLoadRoot save_load_root = prefab.GetComponent< SaveLoadRoot >();
            if( save_load_root != null )
                save_load_root.DeclareOptionalComponent< cNetworkIdentity >();

            prefab.AddOrGet< cNetworkIdentity >();
        }
    }
}