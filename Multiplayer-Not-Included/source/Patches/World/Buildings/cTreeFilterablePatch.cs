using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World;
using MultiplayerNotIncluded.source.Networking.Components;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cTreeFilterablePatch
    {
        public static bool s_skip_send = false;

        [HarmonyPrefix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( TreeFilterable ), nameof( TreeFilterable.AddTagToFilter ) )]
        [HarmonyPatch( new[] { typeof( Tag ) } )]
        private static void addTagToFilter( Tag t, TreeFilterable __instance ) => changeTagFilter( true, t, __instance );

        [HarmonyPrefix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( TreeFilterable ), nameof( TreeFilterable.RemoveTagFromFilter ) )]
        [HarmonyPatch( new[] { typeof( Tag ) } )]
        private static void removeTagFromFilter( Tag t, TreeFilterable __instance ) => changeTagFilter( false, t, __instance );

        private static void changeTagFilter( bool _add, Tag t, TreeFilterable _instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            switch( _add )
            {
                case true when _instance.ContainsTag( t ):
                case false when !_instance.ContainsTag( t ): return;
            }

            cTreeFilterPacket packet;
            cNetworkIdentity  identity = _instance.GetComponent< cNetworkIdentity >();
            if( identity == null )
            {
                int cell = Grid.PosToCell( _instance.transform.localPosition );
                int layer;
                if( !cUtils.tryGetLayer( cell, _instance.gameObject, out layer ) )
                    return;

                packet = cTreeFilterPacket.createStatic( _add, t, cell, layer );
            }
            else
                packet = cTreeFilterPacket.createDynamic( _add, t, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}