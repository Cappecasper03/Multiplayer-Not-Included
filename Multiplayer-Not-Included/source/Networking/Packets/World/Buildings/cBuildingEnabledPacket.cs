using HarmonyLib;
using MultiplayerNotIncluded.Patches.World.Buildings;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cBuildingEnabledPacket : cObjectMenuPacket< BuildingEnabledButton >
    {
        public cBuildingEnabledPacket() : base( ePacketType.kBuildingEnabled ) {}

        public cBuildingEnabledPacket( bool _value, int _cell, int _layer ) : base( ePacketType.kBuildingEnabled, eAction.kStatic, _value, _cell, _layer ) {}
        public cBuildingEnabledPacket( bool _value, int _network_id ) : base( ePacketType.kBuildingEnabled, eAction.kDynamic, _value, _network_id ) {}

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