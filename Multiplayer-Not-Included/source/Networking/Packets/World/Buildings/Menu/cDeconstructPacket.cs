namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings.Menu
{
    public class cDeconstructPacket : cObjectMenuPacket< Deconstructable >
    {
        public cDeconstructPacket() : base( ePacketType.kDeconstruct ) {}

        public cDeconstructPacket( bool _value, int _instance_id ) : base( ePacketType.kDeconstruct, _value, _instance_id ) {}

        protected override void onAction( bool _value, Deconstructable _type_object )
        {
            if( _value )
                _type_object.CancelDeconstruction();
            else
                _type_object.QueueDeconstruction( true );
        }
    }
}