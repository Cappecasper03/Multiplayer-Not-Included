using System;
using OniMultiplayerMod.Networking;
using UnityEngine;

namespace OniMultiplayerMod.DebugTools
{
    public class DebugMenu : MonoBehaviour
    {
        private static DebugMenu _instance;

        private bool _showMenu   = false;
        private Rect _windowRect = new Rect( 10, 10, 250, 300 );

        private Vector2 _scrollPosition = Vector2.zero;

        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterSceneLoad )]
        public static void Initialize()
        {
            if( _instance )
                return;

            GameObject gameObject = new GameObject( "DebugMenu" );
            DontDestroyOnLoad( gameObject );
            _instance = gameObject.AddComponent< DebugMenu >();

            Debug.Log( "[DebugMenu] Initialized" );
        }

        private void Update()
        {
            if( Input.GetKeyDown( KeyCode.Keypad0 ) )
                _showMenu = !_showMenu;
        }

        private void OnGUI()
        {
            if( !_showMenu )
                return;

            GUIStyle style = new GUIStyle( GUI.skin.window ) { padding = new RectOffset( 10, 10, 20, 20 ) };
            _windowRect = GUI.ModalWindow( 888, _windowRect, DrawMenuContents, "Debug Menu", style );
        }

        private void DrawMenuContents( int windowID )
        {
            _scrollPosition = GUILayout.BeginScrollView( _scrollPosition, false, true, GUILayout.Width( _windowRect.width - 20 ), GUILayout.Height( _windowRect.height - 40 ) );

            if( GUILayout.Button( "Create Lobby" ) )
                SteamLobby.Create();

            if( GUILayout.Button( "Leave Lobby" ) )
                SteamLobby.Leave();

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }
    }
}