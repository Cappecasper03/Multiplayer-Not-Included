using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MultiplayerNotIncluded.Patches.Tool;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cDisinfectToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private int      m_cell;

        public ePacketType m_type => ePacketType.kDisinfectTool;

        public cDisinfectToolPacket() {}

        public cDisinfectToolPacket( int _cell ) => m_cell = _cell;

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_cell );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_cell     = _reader.ReadInt32();
        }

        public void onDispatched()
        {
            cDisinfectToolPatch.s_skip_sending = true;
            MethodInfo on_drag_tool = DisinfectTool.Instance.GetType().GetMethod( "OnDragTool", BindingFlags.NonPublic | BindingFlags.Instance );
            on_drag_tool?.Invoke( DisinfectTool.Instance, new object[] { m_cell, 0 } );
            cDisinfectToolPatch.s_skip_sending = false;

            if( !cSession.isHost() )
                return;

            cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }
    }
}