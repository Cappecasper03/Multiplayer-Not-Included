using HarmonyLib;

namespace MultiplayerNotIncluded.Networking.Packets.World.Plants
{
    public class cAutoHarvestPacket : cObjectMenuPacket< HarvestDesignatable >
    {
        public cAutoHarvestPacket() : base( ePacketType.kAutoHarvest ) {}

        public cAutoHarvestPacket( bool _value, int _instance_id ) : base( ePacketType.kAutoHarvest, _value, _instance_id ) {}

        protected override void onAction( bool _value, HarvestDesignatable _type_object )
        {
            _type_object.SetHarvestWhenReady( _value );
        }
    }
}