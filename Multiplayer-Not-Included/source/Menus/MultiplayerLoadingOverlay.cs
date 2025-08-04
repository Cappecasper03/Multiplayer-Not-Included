using System;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace MultiplayerNotIncluded.Menus
{
    public class MultiplayerLoadingOverlay
    {
        public static string Text
        {
            get => _overlay?._textComponent?.text ?? "";
            set
            {
                if( _overlay == null || _overlay._textComponent == null )
                    return;

                _overlay._textComponent.text = value;
            }
        }

        private static   MultiplayerLoadingOverlay _overlay;
        private readonly LocText                   _textComponent;

        private MultiplayerLoadingOverlay()
        {
            Func< float > getScale;
            LoadingOverlay.Load( () => { } );
            LoadingOverlay instance = ( LoadingOverlay )typeof( LoadingOverlay ).GetField( "instance", BindingFlags.NonPublic | BindingFlags.Static )?.GetValue( null );

            if( instance == null )
            {
                DebugTools.Logger.LogWarning( "LoadingOverlay instance is null" );
                return;
            }

            _textComponent = instance.GetComponentInChildren< LocText >();
            if( _textComponent == null )
            {
                DebugTools.Logger.LogWarning( "Couldn't find LocText in LoadingOverlay " );
                return;
            }

            _textComponent.alignment = TextAlignmentOptions.Top;
            _textComponent.margin    = new Vector4( 0, -21, 0, 0 );
            _textComponent.text      = Text;

            KCanvasScaler scaler = instance.GetComponentInParent< KCanvasScaler >();
            if( scaler == null )
            {
                getScale = () => 1;
                DebugTools.Logger.LogWarning( "KCanvasScaler missing." );
            }
            else
                getScale = scaler.GetCanvasScale;

            _textComponent.rectTransform.sizeDelta = new Vector2( Screen.width / getScale(), 0 );
        }

        private void Clear()
        {
            LoadingOverlay.Clear();
        }

        public static void Show( string text )
        {
            _overlay = new MultiplayerLoadingOverlay();
            Text     = text;
        }

        public static void Hide()
        {
            _overlay?.Clear();
            _overlay = null;
        }
    }
}