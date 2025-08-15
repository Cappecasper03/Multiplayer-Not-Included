using System;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Minions;
using MultiplayerNotIncluded.source.Networking.Components;

namespace MultiplayerNotIncluded.source.Patches.Minions
{
    [HarmonyPatch]
    public static class cAssignablePatch
    {
        public static bool s_skip_send = false;

        [HarmonyPrefix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Assignable ), "OnSpawn" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onSpawnPre() => s_skip_send = true;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Assignable ), "OnSpawn" )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void onSpawnPost() => s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Assignable ), nameof( Assignable.Assign ) )]
        [HarmonyPatch( new[] { typeof( IAssignableIdentity ), typeof( AssignableSlotInstance ) } )]
        private static void assign( IAssignableIdentity new_assignee, AssignableSlotInstance specificSlotInstance, Assignable __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            cNetworkIdentity identity = null;
            string           group_id = "";
            switch( new_assignee )
            {
                case AssignmentGroup group: group_id = group.id; break;
                case MinionIdentity minion: identity = minion.GetComponent< cNetworkIdentity >(); break;
                case MinionAssignablesProxy proxy:
                {
                    MinionIdentity minion = proxy.target as MinionIdentity;
                    identity = minion?.GetComponent< cNetworkIdentity >();
                    break;
                }
            }

            int cell = Grid.PosToCell( __instance.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                return;

            cAssignPacket packet;
            if( identity == null )
                packet = cAssignPacket.createGroup( cell, layer, group_id );
            else if( new_assignee is MinionIdentity )
                packet = cAssignPacket.createMinion( cell, layer, identity.getNetworkId() );
            else
                packet = cAssignPacket.createProxy( cell, layer, identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( Assignable ), nameof( Assignable.Unassign ) )]
        [HarmonyPatch( new Type[ 0 ] )]
        private static void unassign( Assignable __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            int cell = Grid.PosToCell( __instance.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, __instance.gameObject, out layer ) )
                return;

            cAssignPacket packet = cAssignPacket.createUnassign( cell, layer );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}