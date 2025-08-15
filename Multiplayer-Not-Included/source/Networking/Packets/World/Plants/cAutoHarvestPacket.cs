namespace MultiplayerNotIncluded.Networking.Packets.World.Plants
{
    public class cAutoHarvestPacket : cObjectMenuPacket< HarvestDesignatable >
    {
        public cAutoHarvestPacket() : base( ePacketType.kAutoHarvest ) {}

        public cAutoHarvestPacket( bool _value, int _cell, int _layer ) : base( ePacketType.kAutoHarvest, eAction.kStatic, _value, _cell, _layer ) {}
        public cAutoHarvestPacket( bool _value, int _network_id ) : base( ePacketType.kAutoHarvest, eAction.kDynamic, _value, _network_id ) {}

        protected override void onAction( bool _value, HarvestDesignatable _type_object )
        {
            _type_object.SetHarvestWhenReady( _value );
        }
    }
}