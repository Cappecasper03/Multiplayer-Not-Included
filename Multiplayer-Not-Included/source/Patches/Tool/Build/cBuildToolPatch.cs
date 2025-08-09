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
            if( !cSession.inSession() || s_skip_sending )
                return;

            BuildingDef  building_def      = AccessTools.Field( typeof( BuildTool ), "def" ).GetValue( __instance ) as BuildingDef;
            IList< Tag > selected_elements = AccessTools.Field( typeof( BuildTool ), "selectedElements" ).GetValue( __instance ) as IList< Tag >;
            string       facade_id         = AccessTools.Field( typeof( BuildTool ), "facadeID" ).GetValue( __instance ) as string;
            Orientation  orientation       = __instance.GetBuildingOrientation;

            if( building_def == null || selected_elements == null || facade_id == null )
                return;

            cBuildToolPacket packet = cBuildToolPacket.createBuilding( building_def.PrefabID, cell, facade_id, orientation, selected_elements );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}