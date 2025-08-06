using MultiplayerNotIncluded.Menus;
using MultiplayerNotIncluded.Networking;
using UnityEngine;

namespace MultiplayerNotIncluded.DebugTools
{
    public class cDebugMenu : MonoBehaviour
    {
        private static cDebugMenu s_instance;

        private bool m_show_menu;
        private Rect m_window_rect = new Rect( 10, 10, 250, 300 );

        private Vector2 m_scroll_position = Vector2.zero;

        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterSceneLoad )]
        public static void initialize()
        {
            if( s_instance )
                return;

            GameObject game_object = new GameObject( "DebugMenu" );
            DontDestroyOnLoad( game_object );
            s_instance = game_object.AddComponent< cDebugMenu >();

            cLogger.logInfo( "Initialized" );
        }

        private void Update()
        {
            if( Input.GetKeyDown( KeyCode.Keypad0 ) )
                m_show_menu = !m_show_menu;
        }

        private void OnGUI()
        {
            if( !m_show_menu )
                return;

            GUIStyle style = new GUIStyle( GUI.skin.window ) { padding = new RectOffset( 10, 10, 20, 20 ) };
            m_window_rect = GUI.ModalWindow( 888, m_window_rect, drawMenuContents, "Debug Menu", style );
        }

        private void drawMenuContents( int _window_id )
        {
            m_scroll_position = GUILayout.BeginScrollView( m_scroll_position, false, true, GUILayout.Width( m_window_rect.width - 20 ),
                                                           GUILayout.Height( m_window_rect.height                               - 40 ) );

            if( GUILayout.Button( "Create Lobby" ) )
                cSteamLobby.create();

            if( GUILayout.Button( "Leave Lobby" ) )
                cSteamLobby.leave();

            if( GUILayout.Button( "Show Overlay" ) )
                cMultiplayerLoadingOverlay.show( "Debug Overlay" );

            if( GUILayout.Button( "Hide Overlay" ) )
                cMultiplayerLoadingOverlay.hide();

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }
    }
}