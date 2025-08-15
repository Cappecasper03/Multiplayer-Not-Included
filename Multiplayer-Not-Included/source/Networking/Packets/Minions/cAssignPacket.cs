using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.source.Networking.Components;
using MultiplayerNotIncluded.source.Patches.Minions;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Minions
{
    public class cAssignPacket : iIPacket
    {
        private enum eAction
        {
            kGroup,
            kMinion,
            kProxy,
            kUnassign,
        }

        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private eAction  m_action;
        private int      m_cell;
        private int      m_layer;
        private string   m_group_id;
        private int      m_network_id;

        public ePacketType m_type => ePacketType.kAssign;

        public static cAssignPacket createGroup( int _cell, int _layer, string _group_id )
        {
            return new cAssignPacket { m_action = eAction.kGroup, m_cell = _cell, m_layer = _layer, m_group_id = _group_id, };
        }

        public static cAssignPacket createMinion( int _cell, int _layer, int _network_id )
        {
            return new cAssignPacket { m_action = eAction.kMinion, m_cell = _cell, m_layer = _layer, m_network_id = _network_id, };
        }

        public static cAssignPacket createProxy( int _cell, int _layer, int _network_id )
        {
            return new cAssignPacket { m_action = eAction.kProxy, m_cell = _cell, m_layer = _layer, m_network_id = _network_id, };
        }

        public static cAssignPacket createUnassign( int _cell, int _layer )
        {
            return new cAssignPacket { m_action = eAction.kUnassign, m_cell = _cell, m_layer = _layer };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_cell );
            _writer.Write( m_layer );

            switch( m_action )
            {
                case eAction.kGroup: _writer.Write( m_group_id ); break;
                case eAction.kMinion:
                case eAction.kProxy: _writer.Write( m_network_id ); break;
                case eAction.kUnassign: break;
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
                case eAction.kGroup: m_group_id = _reader.ReadString(); break;
                case eAction.kMinion:
                case eAction.kProxy: m_network_id = _reader.ReadInt32(); break;
                case eAction.kUnassign: break;
            }
        }

        public void onReceived()
        {
            Assignable assignable = Grid.Objects[ m_cell, m_layer ]?.GetComponent< Assignable >();
            if( assignable == null )
                return;

            cAssignablePatch.s_skip_send = true;

            IAssignableIdentity new_assignee = null;
            switch( m_action )
            {
                case eAction.kGroup:
                {
                    AssignmentGroup group;
                    if( Game.Instance.assignmentManager.assignment_groups.TryGetValue( m_group_id, out group ) )
                        new_assignee = group;
                    break;
                }
                case eAction.kMinion:
                {
                    cNetworkIdentity identity;
                    if( !cNetworkIdentity.tryGetIdentity( m_network_id, out identity ) )
                        break;

                    new_assignee = identity.GetComponent< MinionIdentity >();
                    break;
                }
                case eAction.kProxy:
                {
                    cNetworkIdentity identity;
                    if( !cNetworkIdentity.tryGetIdentity( m_network_id, out identity ) )
                        break;

                    MinionIdentity minion_identity = identity.GetComponent< MinionIdentity >();
                    new_assignee = minion_identity?.assignableProxy.Get();
                    break;
                }
                case eAction.kUnassign: assignable.Unassign(); break;
            }

            if( new_assignee != null )
            {
                assignable.Unassign();
                assignable.Assign( new_assignee );
            }

            cAssignablePatch.s_skip_send = false;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kGroup: cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {( Grid.SceneLayer )m_layer}, {m_group_id}" ); break;
                case eAction.kMinion:
                case eAction.kProxy: cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {( Grid.SceneLayer )m_layer}, {m_network_id}" ); break;
                case eAction.kUnassign: cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {( Grid.SceneLayer )m_layer}" ); break;
            }
        }
    }
}