using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;

namespace MultiplayerNotIncluded.Patches.Tool.Build
{
    [HarmonyPatch]
    public static class cBaseUtilityBuildToolPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( BaseUtilityBuildTool ), "BuildPath" )]
        private static void buildPath( BaseUtilityBuildTool __instance )
        {
            if( !cSession.inSessionAndReady() )
                return;

            IList< Tag >             selected_elements = Traverse.Create( __instance ).Field( "selectedElements" ).GetValue< IList< Tag > >();
            BuildingDef              building_def      = Traverse.Create( __instance ).Field( "def" ).GetValue< BuildingDef >();
            System.Collections.IList instance_path     = Traverse.Create( __instance ).Field( "path" ).GetValue< System.Collections.IList >();
            string                   facade_id         = Traverse.Create( __instance ).Field( "facadeID" ).GetValue< string >();
            if( selected_elements == null || building_def == null || instance_path == null || facade_id == null )
                return;

            IUtilityNetworkMgr manager = building_def.BuildingComplete.GetComponent< IHaveUtilityNetworkMgr >().GetNetworkManager();
            if( manager == null )
                return;

            List< int >                path        = new List< int >();
            List< UtilityConnections > connections = new List< UtilityConnections >();
            foreach( object obj in instance_path )
            {
                Traverse traverse = Traverse.Create( obj );
                int      cell     = traverse.Field( "cell" ).GetValue< int >();
                path.Add( cell );

                connections.Add( manager.GetConnections( cell, false ) );
            }

            cBuildToolPacket packet = cBuildToolPacket.createUtility( building_def.PrefabID, facade_id, selected_elements, path, connections );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}