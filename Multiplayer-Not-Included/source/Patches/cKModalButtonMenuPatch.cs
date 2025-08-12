using HarmonyLib;
using JetBrains.Annotations;
using MultiplayerNotIncluded.Networking;

namespace MultiplayerNotIncluded.source.Patches
{
    [HarmonyPatch]
    public static class cKModalButtonMenuPatch
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        [HarmonyPatch( typeof( KModalButtonMenu ), "OnShow" )]
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