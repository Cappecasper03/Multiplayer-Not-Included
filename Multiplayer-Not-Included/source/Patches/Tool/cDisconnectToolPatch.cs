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
    public static class cDisconnectToolPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( DisconnectTool ), "OnDragComplete" )]
        [HarmonyPatch( new[] { typeof( Vector3 ), typeof( Vector3 ) } )]
        private static void onDragComplete( Vector3 downPos, Vector3 upPos, DisconnectTool __instance )
        {
            if( !cSteamLobby.inLobby() )
                return;

            bool single_disconnect_mode = Traverse.Create( __instance ).Field( "singleDisconnectMode" ).GetValue< bool >();

            if( single_disconnect_mode )
            {
                MethodInfo snap_to_line = __instance.GetType().GetMethod( "SnapToLine", BindingFlags.NonPublic | BindingFlags.Instance );
                object     new_up_pos   = snap_to_line?.Invoke( __instance, new object[] { upPos } );

                if( new_up_pos != null )
                    upPos = ( Vector3 )new_up_pos;
            }

            cDisconnectToolPacket packet = new cDisconnectToolPacket( downPos, upPos );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}