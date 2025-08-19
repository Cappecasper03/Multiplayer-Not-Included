using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cCarePackageContainerPatch
    {
        [HarmonyPrefix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( CarePackageContainer ), "SetAnimator" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static bool setAnimator( CarePackageContainer __instance )
        {
            foreach( ITelepadDeliverable deliverable in cImmigrantPacket.s_deliverables )
            {
                CarePackageInfo care_package = deliverable as CarePackageInfo;
                if( care_package == null )
                    continue;

                cUtils.setField( __instance, "info", care_package );
                cImmigrantPacket.s_deliverables.Remove( deliverable );
                break;
            }

            return true;
        }
    }
}