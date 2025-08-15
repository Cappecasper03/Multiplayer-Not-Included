using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.source.Networking.Components;
using UnityEngine;

namespace MultiplayerNotIncluded.source.Patches.Minions
{
    [HarmonyPatch]
    public static class cMinionConfigPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( MinionConfig ), nameof( MinionConfig.CreatePrefab ) )]
        private static void createPrefab( GameObject __result )
        {
            SaveLoadRoot save_load_root = __result.GetComponent< SaveLoadRoot >();
            if( save_load_root != null )
                save_load_root.DeclareOptionalComponent< cNetworkIdentity >();

            __result.AddOrGet< cNetworkIdentity >();
        }
    }
}