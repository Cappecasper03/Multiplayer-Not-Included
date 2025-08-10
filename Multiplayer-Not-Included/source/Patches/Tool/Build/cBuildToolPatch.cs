using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;

namespace MultiplayerNotIncluded.Patches.Tool.Build
{
    [HarmonyPatch]
    public static class cBuildToolPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( BuildTool ), "TryBuild" )]
        [HarmonyPatch( new[] { typeof( int ) } )]
        private static void tryBuild( int cell, BuildTool __instance )
        {
            if( !cSession.inSessionAndReady() || s_skip_sending )
                return;

            IList< Tag > selected_elements = Traverse.Create( __instance ).Field( "selectedElements" ).GetValue< IList< Tag > >();
            BuildingDef  building_def      = Traverse.Create( __instance ).Field( "def" ).GetValue< BuildingDef >();
            string       facade_id         = Traverse.Create( __instance ).Field( "facadeID" ).GetValue< string >();
            if( building_def == null || selected_elements == null || facade_id == null )
                return;

            cBuildToolPacket packet = cBuildToolPacket.createBuilding( building_def.PrefabID, facade_id, selected_elements, cell, __instance.GetBuildingOrientation );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}