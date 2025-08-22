using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using MultiplayerNotIncluded.source.Networking.Components;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cDoorAccessPacket : iIPacket
    {
        private enum eAction
        {
            kDoor,
            kDefault,
            kMinion,
            kMinionDefault,
        }

        private CSteamID                 m_steam_id = cSession.m_local_steam_id;
        private eAction                  m_action;
        private int                      m_cell;
        private int                      m_layer;
        private Door.ControlState        m_state;
        private AccessControl.Permission m_permission;
        private int                      m_network_id;
        private bool                     m_default;

        public ePacketType m_type => ePacketType.kDoorAccess;

        public static cDoorAccessPacket createDoor( int _cell, int _layer, Door.ControlState _state )
        {
            return new cDoorAccessPacket { m_action = eAction.kDoor, m_state = _state, m_cell = _cell, m_layer = _layer };
        }

        public static cDoorAccessPacket createDefault( int _cell, int _layer, AccessControl.Permission _permission )
        {
            return new cDoorAccessPacket { m_action = eAction.kDefault, m_cell = _cell, m_layer = _layer, m_permission = _permission };
        }

        public static cDoorAccessPacket createMinion( int _cell, int _layer, AccessControl.Permission _permission, int _network_id )
        {
            return new cDoorAccessPacket { m_action = eAction.kMinion, m_cell = _cell, m_layer = _layer, m_permission = _permission, m_network_id = _network_id };
        }

        public static cDoorAccessPacket createMinionDefault( int _cell, int _layer, bool _default, int _network_id )
        {
            return new cDoorAccessPacket { m_action = eAction.kMinionDefault, m_cell = _cell, m_layer = _layer, m_default = _default, m_network_id = _network_id };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_cell );
            _writer.Write( m_layer );

            switch( m_action )
            {
                case eAction.kDoor:    _writer.Write( ( int )m_state ); break;
                case eAction.kDefault: _writer.Write( ( int )m_permission ); break;
                case eAction.kMinion:
                {
                    _writer.Write( ( int )m_permission );
                    _writer.Write( m_network_id );
                    break;
                }
                case eAction.kMinionDefault:
                {
                    _writer.Write( m_default );
                    _writer.Write( m_network_id );
                    break;
                }
            }
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_action   = ( eAction )_reader.ReadInt32();
            m_cell     = _reader.ReadInt32();
            m_layer    = _reader.ReadInt32();

            switch( m_action )
            {
                case eAction.kDoor:    m_state      = ( Door.ControlState )_reader.ReadInt32(); break;
                case eAction.kDefault: m_permission = ( AccessControl.Permission )_reader.ReadInt32(); break;
                case eAction.kMinion:
                {
                    m_permission = ( AccessControl.Permission )_reader.ReadInt32();
                    m_network_id = _reader.ReadInt32();
                    break;
                }
                case eAction.kMinionDefault:
                {
                    m_default    = _reader.ReadBoolean();
                    m_network_id = _reader.ReadInt32();
                    break;
                }
            }
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            if( game_object == null )
                return;

            switch( m_action )
            {
                case eAction.kDoor:
                {
                    Door door = game_object.GetComponent< Door >();
                    if( door == null )
                        return;

                    cDoorPatch.s_skip_send = true;
                    door.QueueStateChange( m_state );
                    cDoorPatch.s_skip_send = false;

                    DoorToggleSideScreen screen = Object.FindObjectOfType< DoorToggleSideScreen >();
                    if( screen != null )
                        Traverse.Create( screen ).Method( "Refresh" )?.GetValue();
                    break;
                }
                case eAction.kDefault:
                {
                    AccessControl access_control = game_object.GetComponent< AccessControl >();
                    if( access_control == null )
                        return;

                    AccessControlSideScreen screen = Object.FindObjectOfType< AccessControlSideScreen >();
                    if( screen != null )
                    {
                        cAccessControlSideScreenPatch.s_skip_send = true;
                        Traverse method = Traverse.Create( screen ).Method( "OnDefaultPermissionChanged", new[] { typeof( MinionAssignablesProxy ), typeof( AccessControl.Permission ) } );
                        method?.GetValue( null, m_permission );
                        cAccessControlSideScreenPatch.s_skip_send = false;
                    }
                    else
                        access_control.DefaultPermission = m_permission;

                    break;
                }
                case eAction.kMinion:
                {
                    AccessControl access_control = game_object.GetComponent< AccessControl >();
                    if( access_control == null )
                        return;

                    cNetworkIdentity identity;
                    if( !cNetworkIdentity.tryGetIdentity( m_network_id, out identity ) )
                        return;

                    Ref< MinionAssignablesProxy > assignable_proxy = identity.GetComponent< MinionIdentity >()?.assignableProxy;
                    if( assignable_proxy == null )
                        return;

                    access_control.SetPermission( assignable_proxy.Get(), m_permission );

                    AccessControlSideScreen screen = Object.FindObjectOfType< AccessControlSideScreen >();
                    if( screen != null )
                    {
                        Traverse method = Traverse.Create( screen ).Method( "Refresh", new[] { typeof( List< MinionAssignablesProxy > ), typeof( bool ) } );
                        method?.GetValue( new List< MinionAssignablesProxy > { assignable_proxy.Get() }, false );
                    }

                    break;
                }
                case eAction.kMinionDefault:
                {
                    AccessControl access_control = game_object.GetComponent< AccessControl >();
                    if( access_control == null )
                        return;

                    cNetworkIdentity identity;
                    if( !cNetworkIdentity.tryGetIdentity( m_network_id, out identity ) )
                        return;

                    Ref< MinionAssignablesProxy > assignable_proxy = identity.GetComponent< MinionIdentity >()?.assignableProxy;
                    if( assignable_proxy == null )
                        return;

                    AccessControlSideScreen screen = Object.FindObjectOfType< AccessControlSideScreen >();
                    if( screen != null )
                    {
                        cAccessControlSideScreenPatch.s_skip_send = true;
                        cUtils.invokeMethod( screen, "OnPermissionDefault", assignable_proxy.Get(), m_default );
                        cAccessControlSideScreenPatch.s_skip_send = false;
                    }
                    else
                    {
                        if( m_default )
                            access_control.ClearPermission( assignable_proxy.Get() );
                        else
                            access_control.SetPermission( assignable_proxy.Get(), access_control.DefaultPermission );
                    }

                    break;
                }
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kDoor:    cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {( Grid.SceneLayer )m_layer}, {m_state}" ); break;
                case eAction.kDefault: cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {( Grid.SceneLayer )m_layer}, {m_permission}" ); break;
                case eAction.kMinion:  cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {( Grid.SceneLayer )m_layer}, {m_permission}, {m_network_id}" ); break;
            }
        }
    }
}