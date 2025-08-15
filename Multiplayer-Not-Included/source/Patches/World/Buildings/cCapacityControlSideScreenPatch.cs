using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;
using Steamworks;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cCapacityControlSideScreenPatch
    {
        public static bool s_skip_send = false;

        private static readonly cDelay s_delay = new cDelay();

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( CapacityControlSideScreen ), "UpdateMaxCapacity" )]
        [HarmonyPatch( new[] { typeof( float ) } )]
        private static void updateMaxCapacity( float newValue, CapacityControlSideScreen __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            KMonoBehaviour mono_behaviour = Traverse.Create( __instance ).Field( "target" ).GetValue< IUserControlledCapacity >() as KMonoBehaviour;
            if( mono_behaviour == null )
                return;

            cCapacityMeterPacket.eComponentType type = cCapacityMeterPacket.eComponentType.kNone;
            switch( mono_behaviour )
            {
                case BaggableCritterCapacityTracker _: type = cCapacityMeterPacket.eComponentType.kBaggableCritterCapacityTracker; break;
                case Bottler _:                        type = cCapacityMeterPacket.eComponentType.kBottler; break;
                case CargoBayCluster _:                type = cCapacityMeterPacket.eComponentType.kCargoBayCluster; break;
                case FuelTank _:                       type = cCapacityMeterPacket.eComponentType.kFuelTank; break;
                case HEPFuelTank _:                    type = cCapacityMeterPacket.eComponentType.kHEPFuelTank; break;
                case ObjectDispenser _:                type = cCapacityMeterPacket.eComponentType.kObjectDispenser; break;
                case OxidizerTank _:                   type = cCapacityMeterPacket.eComponentType.kOxidizerTank; break;
                case RationBox _:                      type = cCapacityMeterPacket.eComponentType.kRationBox; break;
                case Refrigerator _:                   type = cCapacityMeterPacket.eComponentType.kRefrigerator; break;
                case StorageLocker _:                  type = cCapacityMeterPacket.eComponentType.kStorageLocker; break;
            }

            int cell = Grid.PosToCell( mono_behaviour.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, mono_behaviour.gameObject, out layer ) )
                return;

            s_delay.stopAndStart( .5f, () =>
            {
                cCapacityMeterPacket packet = new cCapacityMeterPacket( type, newValue, cell, layer );

                if( cSession.isHost() )
                    cPacketSender.sendToAll( packet );
                else
                    cPacketSender.sendToHost( packet );
            } );
        }
    }
}