using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;

namespace MultiplayerNotIncluded.source.Patches
{
    [HarmonyPatch]
    public static class cKModalScreenPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( KModalScreen ), "OnShow" )]
        [HarmonyPatch( new[] { typeof( bool ) } )]
        private static void onShow( bool show )
        {
            if( !cSession.inSessionAndReady() )
                return;

            if( show && SpeedControlScreen.Instance )
                SpeedControlScreen.Instance.Unpause( false );
        }
    }
}