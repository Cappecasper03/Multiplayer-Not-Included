using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;

namespace MultiplayerNotIncluded.source.Patches
{
    [HarmonyPatch]
    public static class cKModalScreenPatch
    {
        private static bool s_paused;

        [HarmonyPrefix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( KModalScreen ), "OnShow" )]
        [HarmonyPatch( new[] { typeof( bool ) } )]
        private static void onShowPre( bool show )
        {
            if( !cSession.inSessionAndReady() || !SpeedControlScreen.Instance )
                return;

            s_paused = SpeedControlScreen.Instance.IsPaused;
        }

        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( KModalScreen ), "OnShow" )]
        [HarmonyPatch( new[] { typeof( bool ) } )]
        private static void onShowPost( bool show )
        {
            if( !cSession.inSessionAndReady() || !SpeedControlScreen.Instance )
                return;

            switch( show )
            {
                case true when !s_paused:                                          SpeedControlScreen.Instance.Unpause( false ); break;
                case false when s_paused && !SpeedControlScreen.Instance.IsPaused: SpeedControlScreen.Instance.Pause( false ); break;
            }
        }
    }
}