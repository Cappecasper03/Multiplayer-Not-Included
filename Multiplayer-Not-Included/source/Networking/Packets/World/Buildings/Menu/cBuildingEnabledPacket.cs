using HarmonyLib;
using MultiplayerNotIncluded.Patches.World.Buildings.Menu;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings.Menu
{
    public class cBuildingEnabledPacket : cObjectMenuPacket< BuildingEnabledButton >
    {
        public cBuildingEnabledPacket() : base( ePacketType.kBuildingEnabled ) {}

        public cBuildingEnabledPacket( bool _value, int _instance_id ) : base( ePacketType.kBuildingEnabled, _value, _instance_id ) {}

        protected override void onAction( bool _value, BuildingEnabledButton _type_object )
        {
            bool queued_toggle = Traverse.Create( _type_object ).Field( "queuedToggle" ).GetValue< bool >();

            cBuildingEnabledButtonPatch.s_skip_sending = true;
            if( _value != queued_toggle )
                Traverse.Create( _type_object ).Method( "OnMenuToggle" ).GetValue();

            cBuildingEnabledButtonPatch.s_skip_sending = false;
        }
    }
}