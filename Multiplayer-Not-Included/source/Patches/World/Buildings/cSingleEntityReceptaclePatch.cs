using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cSingleEntityReceptaclePatch
    {
        public static bool s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( SingleEntityReceptacle ), nameof( SingleEntityReceptacle.CreateOrder ) )]
        private static void createOrder( Tag entityTag, Tag additionalFilterTag, SingleEntityReceptacle __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            int cell = Grid.PosToCell( __instance.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                return;

            cEntityReceptaclePacket packet = cEntityReceptaclePacket.createOrder( cell, layer, entityTag, additionalFilterTag );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( SingleEntityReceptacle ), nameof( SingleEntityReceptacle.OrderRemoveOccupant ) )]
        private static void orderRemoveOccupant( SingleEntityReceptacle __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            int cell = Grid.PosToCell( __instance.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                return;

            cEntityReceptaclePacket packet = cEntityReceptaclePacket.createRemove( cell, layer );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( SingleEntityReceptacle ), nameof( SingleEntityReceptacle.CancelActiveRequest ) )]
        private static void cancelActiveRequest( SingleEntityReceptacle __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            int cell = Grid.PosToCell( __instance.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                return;

            cEntityReceptaclePacket packet = cEntityReceptaclePacket.createCancel( cell, layer );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}