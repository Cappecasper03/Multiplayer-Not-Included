using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cCharacterSelectionControllerPatch
    {
        [HarmonyPrefix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( CharacterSelectionController ), "InitializeContainers" )]
        private static bool initializeContainersPre( CharacterSelectionController __instance )
        {
            if( !cSession.inSessionAndReady() || cImmigrantPacket.s_deliverables.Count == 0 )
                return true;

            List< ITelepadDeliverableContainer > containers = cUtils.getField< List< ITelepadDeliverableContainer > >( __instance, "containers" );

            cUtils.invokeMethod( __instance, "DisableProceedButton" );
            if( containers != null && containers.Count > 0 )
                return false;

            __instance.OnReplacedEvent = null;
            containers                 = new List< ITelepadDeliverableContainer >();

            int duplicant_count    = 0;
            int care_package_count = 0;
            foreach( ITelepadDeliverable deliverable in cImmigrantPacket.s_deliverables )
            {
                switch( deliverable )
                {
                    case MinionStartingStats _: duplicant_count++; break;
                    case CarePackageInfo _:     care_package_count++; break;
                }
            }

            cUtils.setField( __instance, "numberOfDuplicantOptions",   duplicant_count );
            cUtils.setField( __instance, "numberOfCarePackageOptions", care_package_count );

            GameObject           container_parent              = cUtils.getField< GameObject >( __instance, "containerParent" );
            CharacterContainer   container_prefab              = cUtils.getField< CharacterContainer >( __instance, "containerPrefab" );
            CarePackageContainer care_package_container_prefab = cUtils.getField< CarePackageContainer >( __instance, "carePackageContainerPrefab" );

            for( int index = 0; index < duplicant_count; ++index )
            {
                CharacterContainer character_container = Util.KInstantiateUI< CharacterContainer >( container_prefab.gameObject, container_parent );
                character_container.SetController( __instance );
                character_container.SetReshufflingState( true );
                containers.Add( character_container );
            }

            for( int index = 0; index < care_package_count; ++index )
            {
                CarePackageContainer package_container = Util.KInstantiateUI< CarePackageContainer >( care_package_container_prefab.gameObject, container_parent );
                package_container.SetController( __instance );
                containers.Add( package_container );
                package_container.gameObject.transform.SetSiblingIndex( Random.Range( 0, package_container.transform.parent.childCount ) );
            }

            cUtils.setField( __instance, "containers",           containers );
            cUtils.setField( __instance, "selectedDeliverables", new List< ITelepadDeliverable >() );

            return false;
        }
    }
}