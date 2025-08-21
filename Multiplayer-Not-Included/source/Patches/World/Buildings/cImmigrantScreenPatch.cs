using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cImmigrantScreenPatch
    {
        public static bool s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ImmigrantScreen ), "Initialize" )]
        [HarmonyPatch( new[] { typeof( Telepad ) } )]
        private static void initialize( Telepad telepad )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            List< ITelepadDeliverableContainer > containers = cUtils.getField< List< ITelepadDeliverableContainer > >( ImmigrantScreen.instance, "containers" );
            if( containers == null )
                return;

            int cell = Grid.PosToCell( telepad.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, telepad.gameObject, out layer ) )
                return;

            CoroutineRunner.RunOne( waitForGeneration( cell, layer, containers ) );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ImmigrantScreen ), "OnProceed" )]
        private static void onProceed()
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            Telepad telepad = cUtils.getField< Telepad >( ImmigrantScreen.instance, "telepad" );
            if( telepad == null )
                return;

            List< ITelepadDeliverable > selected_deliverables = cUtils.getField< List< ITelepadDeliverable > >( ImmigrantScreen.instance, "selectedDeliverables" );
            if( selected_deliverables == null )
                return;

            string personality_id  = ( selected_deliverables[ 0 ] as MinionStartingStats )?.personality.Id;
            string care_package_id = ( selected_deliverables[ 0 ] as CarePackageContainer.CarePackageInstanceData )?.info.id;
            string selected_id     = care_package_id ?? personality_id;
            if( string.IsNullOrEmpty( selected_id ) )
                return;

            int cell = Grid.PosToCell( telepad.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, telepad.gameObject, out layer ) )
                return;

            cImmigrantPacket packet = cImmigrantPacket.createSelect( cell, layer, selected_id );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ImmigrantScreen ), "OnRejectionConfirmed" )]
        private static void onRejectionConfirmed()
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            Telepad telepad = cUtils.getField< Telepad >( ImmigrantScreen.instance, "telepad" );
            if( telepad == null )
                return;

            int cell = Grid.PosToCell( telepad.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, telepad.gameObject, out layer ) )
                return;

            cImmigrantPacket packet = cImmigrantPacket.createReject( cell, layer );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        private static IEnumerator waitForGeneration( int _cell, int _layer, List< ITelepadDeliverableContainer > _containers )
        {
            List< ITelepadDeliverable > deliverables = new List< ITelepadDeliverable >();
            do
            {
                yield return null;
                deliverables.Clear();
                foreach( ITelepadDeliverableContainer container in _containers )
                {
                    switch( container )
                    {
                        case CharacterContainer character:      deliverables.Add( character.Stats ); break;
                        case CarePackageContainer care_package: deliverables.Add( care_package.Info ); break;
                    }
                }
            } while( deliverables.Exists( _container => _container == null ) );

            cImmigrantPacket packet = cImmigrantPacket.createSync( _cell, _layer, deliverables );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}