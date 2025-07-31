using Steamworks;
using UnityEngine;

namespace OniMultiplayerMod.Networking
{
    public class PlayerCursor : KMonoBehaviour
    {
        private Camera _camera = null;

        public  CSteamID PlayerId   { get; set; } = CSteamID.Nil;
        private string   PlayerName { get; set; } = string.Empty;

        private Color PlayerColor { get; set; } = Color.white;

        public void Initialize()
        {
            _camera = GameScreenManager.Instance.GetCamera( GameScreenManager.UIRenderTarget.ScreenSpaceCamera );

            PlayerName = SteamFriends.GetFriendPersonaName( PlayerId );
        }
    }
}