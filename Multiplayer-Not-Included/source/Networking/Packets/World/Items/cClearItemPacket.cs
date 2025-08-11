namespace MultiplayerNotIncluded.Networking.Packets.World.Items
{
    public class cClearItemPacket : cObjectMenuPacket< Clearable >
    {
        public cClearItemPacket() : base( ePacketType.kClearItem ) {}

        public cClearItemPacket( bool _value, int _instance_id ) : base( ePacketType.kClearItem, _value, _instance_id ) {}

        protected override void onAction( bool _value, Clearable _type_object )
        {
            if( _value )
                _type_object.MarkForClear();
            else
                _type_object.CancelClearing();
        }
    }
}