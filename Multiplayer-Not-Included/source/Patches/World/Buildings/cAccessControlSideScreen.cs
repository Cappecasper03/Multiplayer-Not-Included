using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World.Buildings;
using MultiplayerNotIncluded.source.Networking.Components;

namespace MultiplayerNotIncluded.Patches.World.Buildings
{
    [HarmonyPatch]
    public static class cAccessControlSideScreenPatch
    {
        public static bool s_skip_send = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( AccessControlSideScreen ), "OnDefaultPermissionChanged" )]
        [HarmonyPatch( new[] { typeof( MinionAssignablesProxy ), typeof( AccessControl.Permission ) } )]
        private static void onDefaultPermissionChanged( MinionAssignablesProxy identity, AccessControl.Permission permission, AccessControlSideScreen __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            AccessControl access_control = Traverse.Create( __instance ).Field( "target" ).GetValue< AccessControl >();
            if( access_control == null )
                return;

            int cell = Grid.PosToCell( access_control.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, access_control.gameObject, out layer ) )
                return;

            cDoorAccessPacket packet = cDoorAccessPacket.createDefault( cell, layer, permission );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( AccessControlSideScreen ), "OnPermissionChanged" )]
        [HarmonyPatch( new[] { typeof( MinionAssignablesProxy ), typeof( AccessControl.Permission ) } )]
        private static void onPermissionChanged( MinionAssignablesProxy identity, AccessControl.Permission permission, AccessControlSideScreen __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            AccessControl access_control = Traverse.Create( __instance ).Field( "target" ).GetValue< AccessControl >();
            if( access_control == null )
                return;

            cNetworkIdentity network_identity = ( identity.target as KMonoBehaviour )?.GetComponent< cNetworkIdentity >();
            if( network_identity == null )
                return;

            int cell = Grid.PosToCell( access_control.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, access_control.gameObject, out layer ) )
                return;

            cDoorAccessPacket packet = cDoorAccessPacket.createMinion( cell, layer, permission, network_identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( AccessControlSideScreen ), "OnPermissionDefault" )]
        [HarmonyPatch( new[] { typeof( MinionAssignablesProxy ), typeof( bool ) } )]
        private static void onPermissionDefault( MinionAssignablesProxy identity, bool isDefault, AccessControlSideScreen __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_send )
                return;

            AccessControl access_control = Traverse.Create( __instance ).Field( "target" ).GetValue< AccessControl >();
            if( access_control == null )
                return;

            cNetworkIdentity network_identity = ( identity.target as KMonoBehaviour )?.GetComponent< cNetworkIdentity >();
            if( network_identity == null )
                return;

            int cell = Grid.PosToCell( access_control.transform.localPosition );
            int layer;
            if( !cUtils.tryGetLayer( cell, access_control.gameObject, out layer ) )
                return;

            cDoorAccessPacket packet = cDoorAccessPacket.createMinionDefault( cell, layer, isDefault, network_identity.getNetworkId() );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}