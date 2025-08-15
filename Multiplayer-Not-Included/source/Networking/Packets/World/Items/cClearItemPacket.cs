namespace MultiplayerNotIncluded.Networking.Packets.World.Items
{
    public class cClearItemPacket : cObjectMenuPacket< Clearable >
    {
        public cClearItemPacket() : base( ePacketType.kClearItem ) {}

        public cClearItemPacket( bool _value, int _cell, int _layer ) : base( ePacketType.kClearItem, eAction.kStatic, _value, _cell, _layer ) {}
        public cClearItemPacket( bool _value, int _network_id ) : base( ePacketType.kClearItem, eAction.kDynamic, _value, _network_id ) {}

        protected override void onAction( bool _value, Clearable _type_object )
        {
            if( _value )
                _type_object.MarkForClear();
            else
                _type_object.CancelClearing();
        }
    }
}