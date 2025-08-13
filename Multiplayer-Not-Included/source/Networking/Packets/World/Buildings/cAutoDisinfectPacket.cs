using HarmonyLib;
using MultiplayerNotIncluded.Patches.World.Buildings;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cAutoDisinfectPacket : cObjectMenuPacket< AutoDisinfectable >
    {
        public cAutoDisinfectPacket() : base( ePacketType.kAutoDisinfect ) {}

        public cAutoDisinfectPacket( bool _value, int _cell, int _layer ) : base( ePacketType.kAutoDisinfect, eAction.kStatic, _value, _cell, _layer ) {}
        public cAutoDisinfectPacket( bool _value, int _network_id ) : base( ePacketType.kAutoDisinfect, eAction.kDynamic, _value, _network_id ) {}

        protected override void onAction( bool _value, AutoDisinfectable _type_object )
        {
            cAutoDisinfectablePatch.s_skip_sending = true;
            if( _value )
                Traverse.Create( _type_object ).Method( "EnableAutoDisinfect" )?.GetValue();
            else
                Traverse.Create( _type_object ).Method( "DisableAutoDisinfect" )?.GetValue();

            cAutoDisinfectablePatch.s_skip_sending = false;
        }
    }
}