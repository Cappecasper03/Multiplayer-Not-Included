namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cDeconstructPacket : cObjectMenuPacket< Deconstructable >
    {
        public cDeconstructPacket() : base( ePacketType.kDeconstruct ) {}

        public cDeconstructPacket( bool _value, int _cell, int _layer ) : base( ePacketType.kDeconstruct, eAction.kStatic, _value, _cell, _layer ) {}
        public cDeconstructPacket( bool _value, int _network_id ) : base( ePacketType.kDeconstruct, eAction.kDynamic, _value, _network_id ) {}

        protected override void onAction( bool _value, Deconstructable _type_object )
        {
            if( _value )
                _type_object.CancelDeconstruction();
            else
                _type_object.QueueDeconstruction( true );
        }
    }
}