using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cActiveRangeSideScreenPatch
    {
        public static bool s_skip_send = false;

        private static readonly cDelay s_delay = new cDelay();

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ActiveRangeSideScreen ), "OnActivateValueChanged" )]
        [HarmonyPatch( new[] { typeof( float ) } )]
        private static void onActivateValueChanged( float new_value, ActiveRangeSideScreen __instance ) => changeValue( true, new_value, __instance );

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ActiveRangeSideScreen ), "OnDeactivateValueChanged" )]
        [HarmonyPatch( new[] { typeof( float ) } )]
        private static void onDeactivateValueChanged( float new_value, ActiveRangeSideScreen __instance ) => changeValue( false, new_value, __instance );

        private static void changeValue( bool _active, float _value, ActiveRangeSideScreen _instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            KMonoBehaviour mono_behaviour = Traverse.Create( _instance ).Field( "target" ).GetValue< IActivationRangeTarget >() as KMonoBehaviour;
            if( mono_behaviour == null )
                return;

            int cell = Grid.PosToCell( mono_behaviour.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, mono_behaviour.gameObject, out layer ) )
                return;

            s_delay.stopAndStart( .5f, () =>
            {
                cActivationRangePacket packet = new cActivationRangePacket( _active, _value, cell, layer );

                if( cSession.isHost() )
                    cPacketSender.sendToAll( packet );
                else
                    cPacketSender.sendToHost( packet );
            } );
        }
    }
}