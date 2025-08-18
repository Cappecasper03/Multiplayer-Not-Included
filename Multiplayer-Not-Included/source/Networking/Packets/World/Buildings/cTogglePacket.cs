using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cTogglePacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_value;
        private int      m_cell;
        private int      m_layer;

        public ePacketType m_type => ePacketType.kToggle;

        public cTogglePacket() {}

        public cTogglePacket( bool _value, int _cell, int _layer )
        {
            m_value = _value;
            m_cell  = _cell;
            m_layer = _layer;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_value );
            _writer.Write( m_cell );
            _writer.Write( m_layer );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_value    = _reader.ReadBoolean();
            m_cell     = _reader.ReadInt32();
            m_layer    = _reader.ReadInt32();
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            if( game_object == null )
                return;

            IPlayerControlledToggle controlled_toggle = game_object.GetComponent< IPlayerControlledToggle >();
            if( controlled_toggle == null )
                return;

            if( SpeedControlScreen.Instance.IsPaused && controlled_toggle.ToggleRequested == m_value )
                return;
            if( controlled_toggle.ToggledOn() == m_value )
                return;

            PlayerControlledToggleSideScreen screen = Object.FindObjectOfType< PlayerControlledToggleSideScreen >();
            if( screen != null && screen.target == controlled_toggle )
            {
                cPlayerControlledToggleSideScreenPatch.s_skip_send = true;
                Traverse.Create( screen ).Method( "ClickToggle" )?.GetValue();
                cPlayerControlledToggleSideScreenPatch.s_skip_send = false;
            }
            else
            {
                StatusItem status_item = new StatusItem( nameof( PlayerControlledToggleSideScreen ), "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID );

                if( SpeedControlScreen.Instance.IsPaused )
                {
                    controlled_toggle.ToggleRequested = !controlled_toggle.ToggleRequested;
                    if( controlled_toggle.ToggleRequested && SpeedControlScreen.Instance.IsPaused )
                        controlled_toggle.GetSelectable().SetStatusItem( Db.Get().StatusItemCategories.Main, status_item, this );
                    else
                        controlled_toggle.GetSelectable().SetStatusItem( Db.Get().StatusItemCategories.Main, null );
                }
                else
                {
                    controlled_toggle.ToggledByPlayer();
                    controlled_toggle.ToggleRequested = false;
                    controlled_toggle.GetSelectable().RemoveStatusItem( status_item );
                }
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_value}, {m_cell}, {( Grid.SceneLayer )m_layer}" );
    }
}