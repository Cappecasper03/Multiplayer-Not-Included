using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cFilterablePatch
    {
        public static bool s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Filterable ), "OnFilterChanged" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onFilterChanged( Filterable __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            Tag filter = Traverse.Create( __instance ).Field( "selectedTag" ).GetValue< Tag >();
            if( filter == null )
                return;

            int cell = Grid.PosToCell( __instance.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                return;

            cFilterPacket packet = new cFilterPacket( filter, cell, layer );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}