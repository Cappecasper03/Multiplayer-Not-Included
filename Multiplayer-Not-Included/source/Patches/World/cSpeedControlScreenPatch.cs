using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.World;

namespace MultiplayerNotIncluded.Patches.World
{
    [HarmonyPatch]
    public static class cSpeedControlScreenPatch
    {
        public static bool s_skip_sending = false;

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( SpeedControlScreen ), nameof( SpeedControlScreen.TogglePause ) )]
        private static void togglePause( bool playsound )
        {
            if( !cSteamLobby.inLobby() || s_skip_sending )
                return;

            bool               is_paused = SpeedControlScreen.Instance.IsPaused;
            int                speed     = SpeedControlScreen.Instance.GetSpeed();
            cSpeedChangePacket packet    = new cSpeedChangePacket( is_paused, speed );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( SpeedControlScreen ), nameof( SpeedControlScreen.SetSpeed ) )]
        private static void setSpeed( int Speed )
        {
            if( !cSteamLobby.inLobby() || s_skip_sending )
                return;

            bool               is_paused = SpeedControlScreen.Instance.IsPaused;
            cSpeedChangePacket packet    = new cSpeedChangePacket( is_paused, Speed );

            if( cSession.isHost() )
                cPacketSender.sendToAll( packet );
            else
                cPacketSender.sendToHost( packet );
        }
    }
}