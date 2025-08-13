using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World;
using MultiplayerNotIncluded.source.Networking.Components;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.World
{
    [HarmonyPatch]
    public static class cUserMenuScreenPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( UserMenuScreen ), "OnPriorityClicked" )]
        [HarmonyPatch( new[] { typeof( PrioritySetting ) } )]
        private static void onPriorityClicked( PrioritySetting priority, UserMenuScreen __instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            GameObject game_object = Traverse.Create( __instance ).Field( "selected" ).GetValue< GameObject >();
            if( game_object == null )
                return;

            cPriorityPacket  packet;
            cNetworkIdentity identity = game_object.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( game_object.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, game_object, out layer ) )
                    return;

                packet = cPriorityPacket.createStatic( priority, cell, layer );
            }
            else
                packet = cPriorityPacket.createDynamic( priority, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}