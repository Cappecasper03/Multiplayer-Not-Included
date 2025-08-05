using System;
using System.Reflection;
using MultiplayerNotIncluded.DebugTools;
using TMPro;
using UnityEngine;

namespace MultiplayerNotIncluded.Menus
{
    public class cMultiplayerLoadingOverlay
    {
        private static string m_text
        {
            get => s_overlay?.m_text_component?.text ?? "";
            set
            {
                if( s_overlay == null || s_overlay.m_text_component == null )
                    return;

                s_overlay.m_text_component.text = value;
            }
        }

        private static   cMultiplayerLoadingOverlay s_overlay;
        private readonly LocText                    m_text_component;

        private static bool s_main_menu_active    = false;
        private static bool s_pause_screen_active = false;

        private cMultiplayerLoadingOverlay()
        {
            Func< float > get_scale;
            LoadingOverlay.Load( () => {} );
            LoadingOverlay instance = ( LoadingOverlay )typeof( LoadingOverlay ).GetField( "instance", BindingFlags.NonPublic | BindingFlags.Static )?.GetValue( null );

            if( instance == null )
            {
                cLogger.logWarning( "LoadingOverlay instance is null" );
                return;
            }

            m_text_component = instance.GetComponentInChildren< LocText >();
            if( m_text_component == null )
            {
                cLogger.logWarning( "Couldn't find LocText in LoadingOverlay " );
                return;
            }

            m_text_component.alignment = TextAlignmentOptions.Top;
            m_text_component.margin    = new Vector4( 0, -21, 0, 0 );
            m_text_component.text      = m_text;

            KCanvasScaler scaler = instance.GetComponentInParent< KCanvasScaler >();
            if( scaler == null )
            {
                get_scale = () => 1;
                cLogger.logWarning( "KCanvasScaler missing." );
            }
            else
                get_scale = scaler.GetCanvasScale;

            m_text_component.rectTransform.sizeDelta = new Vector2( Screen.width / get_scale(), 0 );
        }

        private void clear() { LoadingOverlay.Clear(); }

        public static void show( string _text )
        {
            s_overlay = new cMultiplayerLoadingOverlay();
            m_text    = _text;

            if( MainMenu.Instance.isActiveAndEnabled )
            {
                s_main_menu_active = true;
                MainMenu.Instance.Show( false );
            }
            else if( PauseScreen.Instance.isActiveAndEnabled )
            {
                s_pause_screen_active = true;
                PauseScreen.Instance.Show( false );
            }
        }

        public static void hide()
        {
            s_overlay?.clear();
            s_overlay = null;

            if( s_main_menu_active )
            {
                s_main_menu_active = false;
                MainMenu.Instance.Show();
            }
            else if( s_pause_screen_active )
            {
                s_pause_screen_active = false;
                PauseScreen.Instance.Show();
            }
        }
    }
}