using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cSliderSetPatch
    {
        public static bool s_skip_send = false;

        private static readonly cDelay s_delay = new cDelay();

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( SliderSet ), "SetValue" )]
        [HarmonyPatch( new[] { typeof( float ) } )]
        private static void setValue( float value, SliderSet __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            ISliderControl slider_control = Traverse.Create( __instance ).Field( "target" ).GetValue< ISliderControl >();
            KMonoBehaviour mono_behaviour = slider_control as KMonoBehaviour;
            if( mono_behaviour == null )
                return;

            cSliderSetPacket.eAction type = cSliderSetPacket.eAction.kNone;
            switch( Object.FindObjectOfType< SideScreenContent >() )
            {
                case SingleSliderSideScreen _: type = cSliderSetPacket.eAction.kSingle; break;
                case DualSliderSideScreen _:   type = cSliderSetPacket.eAction.kDual; break;
                case MultiSliderSideScreen _:  type = cSliderSetPacket.eAction.kMulti; break;
                case IntSliderSideScreen _:    type = cSliderSetPacket.eAction.kInt; break;
            }

            int cell = Grid.PosToCell( mono_behaviour.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, mono_behaviour.gameObject, out layer ) )
                return;

            s_delay.stopAndStart( .5f, () =>
            {
                cSliderSetPacket packet = new cSliderSetPacket( type, slider_control.GetSliderValue( __instance.index ), cell, layer );

                if( cSession.isHost() )
                    cPacketSender.sendToAll( packet );
                else
                    cPacketSender.sendToHost( packet );
            } );
        }
    }
}