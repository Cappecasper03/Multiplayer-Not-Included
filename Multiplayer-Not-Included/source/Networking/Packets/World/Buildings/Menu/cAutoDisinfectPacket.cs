using HarmonyLib;
using MultiplayerNotIncluded.Patches.World.Buildings.Menu;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings.Menu
{
    public class cAutoDisinfectPacket : cObjectMenuPacket< AutoDisinfectable >
    {
        public cAutoDisinfectPacket() : base( ePacketType.kAutoDisinfect ) {}

        public cAutoDisinfectPacket( bool _value, int _instance_id ) : base( ePacketType.kAutoDisinfect, _value, _instance_id ) {}

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