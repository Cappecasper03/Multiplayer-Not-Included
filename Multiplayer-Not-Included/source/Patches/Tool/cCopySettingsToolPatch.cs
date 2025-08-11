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
        private static void onDragComplete( int cell, int distFromOrigin, CopySettingsTool __instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            GameObject game_object = Traverse.Create( __instance ).Field( "sourceGameObject" ).GetValue< GameObject >();
            KPrefabID  prefab_id   = game_object?.GetComponent< KPrefabID >();
            if( prefab_id == null )
                return;

            cCopySettingsToolPacket packet = new cCopySettingsToolPacket( cell, prefab_id.InstanceID );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}