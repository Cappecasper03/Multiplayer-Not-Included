using MultiplayerNotIncluded.Menus;
using MultiplayerNotIncluded.Networking;
using MultiplayerNotIncluded.Networking.Packets;
using MultiplayerNotIncluded.Networking.Packets.Players;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerNotIncluded.source.Networking.Components
{
    public class cPlayerCursorComponent : KMonoBehaviour
    {
        public Vector3 m_target_position;

        private CSteamID m_steam_id;

        private Camera m_camera;

        private Texture2D       m_texture;
        private Image           m_image;
        private TextMeshProUGUI m_text;

        private float m_last_send_time;

        [MyCmpAdd]
        private readonly Canvas m_canvas = null;

        private Canvas m_camera_canvas;

        public void initialize( CSteamID _steam_id )
        {
            m_steam_id = _steam_id;

            if( m_steam_id == cSession.m_local_steam_id )
                return;

            m_camera        = GameScreenManager.Instance.GetCamera( GameScreenManager.UIRenderTarget.ScreenSpaceCamera );
            m_camera_canvas = GameScreenManager.Instance.ssCameraCanvas?.GetComponent< Canvas >();

            m_texture = Assets.GetTexture( "cursor_arrow" );
            GameObject cursor = new GameObject( name );

            m_image = createImage( cursor, m_texture );

            m_text = createText( cursor, new Vector3( m_texture.width, -m_texture.height, 0 ) );

            m_image.transform.SetSiblingIndex( 0 );
            m_text.transform.SetSiblingIndex( 1 );

            cursor.transform.SetParent( transform, false );
            gameObject.SetLayerRecursively( LayerMask.NameToLayer( "UI" ) );

            m_text.text = SteamFriends.GetFriendPersonaName( m_steam_id );

            m_canvas.overrideSorting = true;
            m_canvas.sortingOrder    = 100;
        }

        private void Update()
        {
            if( !cSteamLobby.inLobby() || !cUtils.isInGame() || cMultiplayerLoadingOverlay.isVisible() )
                return;

            if( m_steam_id != cSession.m_local_steam_id )
            {
                transform.position = Vector3.MoveTowards( transform.position, m_target_position, 1 );
                return;
            }

            float time = Time.realtimeSinceStartup;
            if( m_last_send_time + .01f > time )
                return;

            m_last_send_time = time;

            cPlayerCursorPacket packet = new cPlayerCursorPacket( getCursorPosition() );
            if( cSession.isHost() )
                cPacketSender.sendToAll( packet, eSteamNetworkingSend.kUnreliable );
            else
                cPacketSender.sendToHost( packet, eSteamNetworkingSend.kUnreliable );
        }

        public void setVisibility( bool _visible )
        {
            if( m_image == null || m_text == null )
                return;

            Color color = m_image.color;
            color.a       = _visible ? 1 : 0;
            m_image.color = color;

            color        = m_text.color;
            color.a      = _visible ? 1 : 0;
            m_text.color = color;
        }

        private Image createImage( GameObject _parent, Texture2D _texture )
        {
            GameObject image_object = new GameObject( name ) { transform = { parent = _parent.transform } };

            RectTransform rect_transform = image_object.AddComponent< RectTransform >();
            rect_transform.sizeDelta = new Vector2( m_texture.width, m_texture.height );
            rect_transform.pivot     = new Vector2( 0,               1 );

            Image image_component = image_object.AddComponent< Image >();
            image_component.sprite        = Sprite.Create( _texture, new Rect( 0, 0, _texture.width, _texture.height ), Vector2.zero );
            image_component.raycastTarget = false;

            return image_component;
        }

        private TextMeshProUGUI createText( GameObject _parent, Vector3 _offset )
        {
            GameObject text_object = new GameObject( $"{name}_Name" ) { transform = { parent = _parent.transform } };

            RectTransform rect_transform = text_object.AddComponent< RectTransform >();
            rect_transform.sizeDelta = new Vector2( 50, 50 );
            rect_transform.pivot     = new Vector2( 0,  1 );
            rect_transform.position  = _offset;

            TextMeshProUGUI text_component = text_object.AddComponent< TextMeshProUGUI >();
            text_component.fontSize           = 14;
            text_component.font               = Localization.FontAsset;
            text_component.color              = Color.white;
            text_component.raycastTarget      = false;
            text_component.enableWordWrapping = false;

            return text_component;
        }

        private Vector3 getCursorPosition()
        {
            if( m_camera == null )
                m_camera = GameScreenManager.Instance.GetCamera( GameScreenManager.UIRenderTarget.ScreenSpaceCamera );

            if( m_camera_canvas == null )
                m_camera_canvas = GameScreenManager.Instance.ssCameraCanvas?.GetComponent< Canvas >();

            Vector3 position = Input.mousePosition;
            position.z = m_camera_canvas?.planeDistance ?? 10;

            return m_camera.ScreenToWorldPoint( position );
        }
    }
}