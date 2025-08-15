using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.Tool
{
    [HarmonyPatch]
    public static class cCopySettingsToolPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( CopySettingsTool ), "OnDragTool" )]
        [HarmonyPatch( new[] { typeof( int ), typeof( int ) } )]
        private static void onDragTool( int cell, int distFromOrigin, CopySettingsTool __instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            GameObject game_object = Traverse.Create( __instance ).Field( "sourceGameObject" ).GetValue< GameObject >();
            if( game_object == null )
                return;

            int source_cell = Grid.PosToCell( game_object.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( source_cell, game_object, out layer ) )
                return;

            cCopySettingsToolPacket packet = new cCopySettingsToolPacket( cell, source_cell, layer );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}