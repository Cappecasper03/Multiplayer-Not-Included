using MultiplayerNotIncluded.Patches.World.Creatures;

namespace MultiplayerNotIncluded.Networking.Packets.World.Creatures
{
    public class cAttackCreaturePacket : cObjectMenuPacket< FactionAlignment >
    {
        public cAttackCreaturePacket() : base( ePacketType.kAttackCreature ) {}

        public cAttackCreaturePacket( bool _value, int _cell, int _layer ) : base( ePacketType.kAttackCreature, eAction.kStatic, _value, _cell, _layer ) {}
        public cAttackCreaturePacket( bool _value, int _network_id ) : base( ePacketType.kAttackCreature, eAction.kDynamic, _value, _network_id ) {}

        protected override void onAction( bool _value, FactionAlignment _type_object )
        {
            cFactionAlignmentPatch.s_skip_send = true;
            _type_object.SetPlayerTargeted( _value );
            cFactionAlignmentPatch.s_skip_send = false;
        }
    }
}