using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using MultiplayerNotIncluded.source.Networking.Components;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.Tool
{
    [HarmonyPatch]
    public static class cMoveToLocationToolPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( MoveToLocationTool ), "SetMoveToLocation" )]
        [HarmonyPatch( new[] { typeof( int ) } )]
        private static void setMoveToLocation( int target_cell, MoveToLocationTool __instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            Navigator navigator = Traverse.Create( __instance ).Field( "targetNavigator" ).GetValue< Navigator >();
            Movable   movable   = Traverse.Create( __instance ).Field( "targetMovable" ).GetValue< Movable >();

            GameObject game_object = navigator?.gameObject ?? movable?.gameObject;
            if( game_object == null )
                return;

            cMoveToLocationToolPacket packet;
            cNetworkIdentity          identity = game_object.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( game_object.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, game_object.gameObject, out layer ) )
                    return;

                packet = cMoveToLocationToolPacket.createStatic( target_cell, cell, layer );
            }
            else

                packet = cMoveToLocationToolPacket.createDynamic( target_cell, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}