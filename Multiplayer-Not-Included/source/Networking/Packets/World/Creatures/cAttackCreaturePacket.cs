using MultiplayerNotIncluded.Patches.World.Creatures;

namespace MultiplayerNotIncluded.Networking.Packets.World.Creatures
{
    public class cAttackCreaturePacket : cObjectMenuPacket< FactionAlignment >
    {
        public cAttackCreaturePacket() : base( ePacketType.kAttackCreature ) {}

        public cAttackCreaturePacket( bool _value, int _instance_id ) : base( ePacketType.kAttackCreature, _value, _instance_id ) {}

        protected override void onAction( bool _value, FactionAlignment _type_object )
        {
            cFactionAlignmentPatch.s_skip_send = true;
            _type_object.SetPlayerTargeted( _value );
            cFactionAlignmentPatch.s_skip_send = false;
        }
    }
}