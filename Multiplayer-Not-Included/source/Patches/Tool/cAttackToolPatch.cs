using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.Tool
{
    [HarmonyPatch]
    public static class cAttackToolPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( AttackTool ), "OnDragComplete" )]
        [HarmonyPatch( new[] { typeof( Vector3 ), typeof( Vector3 ) } )]
        private static void onDragComplete( Vector3 downPos, Vector3 upPos, AttackTool __instance )
        {
            if( !cSession.inSession() )
                return;

            Traverse get_regularized_pos = Traverse.Create( __instance ).Method( "GetRegularizedPos", new[] { typeof( Vector2 ), typeof( bool ) } );

            object min_object = get_regularized_pos?.GetValue( Vector2.Min( downPos, upPos ), true );
            object max_object = get_regularized_pos?.GetValue( Vector2.Max( downPos, upPos ), false );

            if( min_object == null || max_object == null )
                return;

            cAttackToolPacket packet = new cAttackToolPacket( ( Vector2 )min_object, ( Vector2 )max_object );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}