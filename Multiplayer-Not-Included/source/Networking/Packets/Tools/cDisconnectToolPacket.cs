using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cDisconnectToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private Vector3  m_down_pos;
        private Vector3  m_up_pos;

        public ePacketType m_type => ePacketType.kDisconnectTool;

        public cDisconnectToolPacket() {}

        public cDisconnectToolPacket( Vector3 _down_pos, Vector3 _up_pos )
        {
            m_down_pos = _down_pos;
            m_up_pos   = _up_pos;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_down_pos );
            _writer.Write( m_up_pos );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_down_pos = _reader.ReadVector3();
            m_up_pos   = _reader.ReadVector3();
        }

        public void onDispatched()
        {
            MethodInfo run_on_region           = DisconnectTool.Instance.GetType().GetMethod( "RunOnRegion",           BindingFlags.NonPublic | BindingFlags.Instance );
            MethodInfo clear_visualizers       = DisconnectTool.Instance.GetType().GetMethod( "ClearVisualizers",      BindingFlags.NonPublic | BindingFlags.Instance );
            MethodInfo disconnect_cells_action = DisconnectTool.Instance.GetType().GetMethod( "DisconnectCellsAction", BindingFlags.NonPublic | BindingFlags.Instance );

            var action = ( Action< int, GameObject, IHaveUtilityNetworkMgr, UtilityConnections > )
                disconnect_cells_action?.CreateDelegate( typeof( Action< int, GameObject, IHaveUtilityNetworkMgr, UtilityConnections > ), DisconnectTool.Instance );

            run_on_region?.Invoke( DisconnectTool.Instance, new object[] { m_down_pos, m_up_pos, action } );
            clear_visualizers?.Invoke( DisconnectTool.Instance, new object[] {} );

            if( !cSession.isHost() )
                return;

            cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }
    }
}