using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cCharacterContainerPatch
    {
        [HarmonyPrefix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( CharacterContainer ), "SetAnimator" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static bool setAnimator( CharacterContainer __instance )
        {
            foreach( ITelepadDeliverable deliverable in cImmigrantPacket.s_deliverables )
            {
                MinionStartingStats stats = deliverable as MinionStartingStats;
                if( stats == null )
                    continue;

                cUtils.setField( __instance, "stats", stats );
                cImmigrantPacket.s_deliverables.Remove( deliverable );
                break;
            }

            return true;
        }
    }
}