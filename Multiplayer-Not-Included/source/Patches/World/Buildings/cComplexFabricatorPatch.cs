using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cComplexFabricatorPatch
    {
        public static bool s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ComplexFabricator ), nameof( ComplexFabricator.IncrementRecipeQueueCount ) )]
        private static void incrementRecipeQueueCount( ComplexRecipe recipe, ComplexFabricator __instance ) => changeRecipeQueueCount( false, recipe, 1, __instance );

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ComplexFabricator ), nameof( ComplexFabricator.DecrementRecipeQueueCount ) )]
        private static void decrementRecipeQueueCount( ComplexRecipe recipe, ComplexFabricator __instance ) => changeRecipeQueueCount( false, recipe, -1, __instance );

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( ComplexFabricator ), nameof( ComplexFabricator.SetRecipeQueueCount ) )]
        private static void setRecipeQueueCount( ComplexRecipe recipe, int count, ComplexFabricator __instance ) => changeRecipeQueueCount( true, recipe, count, __instance );

        private static void changeRecipeQueueCount( bool _set, ComplexRecipe _recipe, int _count, ComplexFabricator _instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            int cell = Grid.PosToCell( _instance.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, _instance.gameObject, out layer ) )
                return;

            cFabricatorPacket packet = new cFabricatorPacket( _set, _recipe.id, _count, cell, layer );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}