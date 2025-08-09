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
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( BaseUtilityBuildTool ), "BuildPath" )]
        private static void buildPath( BaseUtilityBuildTool __instance )
        {
            if( !cSession.inSession() || s_skip_sending )
                return;

            BuildingDef              building_def  = Traverse.Create( __instance ).Field( "def" ).GetValue< BuildingDef >();
            System.Collections.IList instance_path = Traverse.Create( __instance ).Field( "path" ).GetValue< System.Collections.IList >();
            if( building_def == null || instance_path == null )
                return;

            List< Tuple< int, bool > > path = new List< Tuple< int, bool > >();
            foreach( object obj in instance_path )
            {
                Traverse traverse = Traverse.Create( obj );
                int      cell     = traverse.Field( "cell" ).GetValue< int >();
                bool     valid    = traverse.Field( "valid" ).GetValue< bool >();
                path.Add( new Tuple< int, bool >( cell, valid ) );
            }

            cBuildToolPacket packet = cBuildToolPacket.createUtility( building_def.PrefabID, path );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}