using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Tools;
using UnityEngine;

namespace MultiplayerNotIncluded.Patches.Tool
{
    [HarmonyPatch]
    public static class cAttackToolPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( AttackTool ), "OnDragComplete" )]
        [HarmonyPatch( new[] { typeof( Vector3 ), typeof( Vector3 ) } )]
        private static void onDragTool( Vector3 downPos, Vector3 upPos )
        {
            if( !cSteamLobby.inLobby() )
                return;

            Vector2 min = getRegularizedPos( Vector2.Min( downPos, upPos ), true );
            Vector2 max = getRegularizedPos( Vector2.Max( downPos, upPos ), false );

            cAttackToolPacket packet = new cAttackToolPacket( min, max );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        private static Vector2 getRegularizedPos( Vector2 _input, bool _minimize )
        {
            Vector3 size = new Vector3( Grid.HalfCellSizeInMeters, Grid.HalfCellSizeInMeters, 0 );
            return Grid.CellToPosCCC( Grid.PosToCell( _input ), Grid.SceneLayer.Background ) + ( _minimize ? -size : size );
        }
    }
}