using MultiplayerNotIncluded.Patches.World.Creatures;

namespace MultiplayerNotIncluded.Networking.Packets.World.Creatures
{
    public class cCaptureCreaturePacket : cObjectMenuPacket< Capturable >
    {
        public cCaptureCreaturePacket() : base( ePacketType.kCaptureCreature ) {}

        public cCaptureCreaturePacket( bool _value, int _instance_id ) : base( ePacketType.kCaptureCreature, _value, _instance_id ) {}

        protected override void onAction( bool _value, Capturable _type_object )
        {
            cCapturablePatch.s_skip_send = true;
            _type_object.MarkForCapture( _value );
            cCapturablePatch.s_skip_send = false;
        }
    }
}