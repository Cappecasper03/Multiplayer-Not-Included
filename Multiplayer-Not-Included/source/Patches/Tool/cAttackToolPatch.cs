using System.Reflection;
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

            MethodInfo get_regularized_pos = __instance.GetType().GetMethod( "GetRegularizedPos", BindingFlags.NonPublic | BindingFlags.Instance );

            object min_object = get_regularized_pos?.Invoke( __instance, new object[] { Vector2.Min( downPos, upPos ), true } );
            object max_object = get_regularized_pos?.Invoke( __instance, new object[] { Vector2.Max( downPos, upPos ), false } );

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