using HarmonyLib;
using MultiplayerNotIncluded.Patches.World.Buildings;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cAutoRepairPacket : cObjectMenuPacket< Repairable >
    {
        public cAutoRepairPacket() : base( ePacketType.kAutoRepair ) {}

        public cAutoRepairPacket( bool _value, int _cell, int _layer ) : base( ePacketType.kAutoRepair, eAction.kStatic, _value, _cell, _layer ) {}
        public cAutoRepairPacket( bool _value, int _network_id ) : base( ePacketType.kAutoRepair, eAction.kDynamic, _value, _network_id ) {}

        protected override void onAction( bool _value, Repairable _type_object )
        {
            cRepairablePatch.s_skip_sending = true;
            if( _value )
                Traverse.Create( _type_object ).Method( "AllowRepair" )?.GetValue();
            else
                _type_object.CancelRepair();

            cRepairablePatch.s_skip_sending = false;
        }
    }
}