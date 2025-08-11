using HarmonyLib;

namespace MultiplayerNotIncluded.Networking.Packets.World.Plants
{
    public class cUprootPacket : cObjectMenuPacket< Uprootable >
    {
        public cUprootPacket() : base( ePacketType.kUproot ) {}

        public cUprootPacket( bool _value, int _instance_id ) : base( ePacketType.kUproot, _value, _instance_id ) {}

        protected override void onAction( bool _value, Uprootable _type_object )
        {
            if( _value )
                _type_object.MarkForUproot( false );
            else
                Traverse.Create( _type_object ).Method( "OnCancel", new[] { typeof( object ) } )?.GetValue( ( object )null );
        }
    }
}