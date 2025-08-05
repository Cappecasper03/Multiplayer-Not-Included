using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cClearToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private int      m_cell;

        public ePacketType m_type => ePacketType.kClearTool;

        public cClearToolPacket() {}

        public cClearToolPacket( int _cell ) => m_cell = _cell;

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
            GameObject cell_object = Grid.Objects[ m_cell, 3 ];
            if( cell_object == null )
                return;

            ObjectLayerListItem object_layer_list_item = cell_object.GetComponent< Pickupable >().objectLayerListItem;
            while( object_layer_list_item != null )
            {
                GameObject game_object = object_layer_list_item.gameObject;
                object_layer_list_item = object_layer_list_item.nextItem;
                if( game_object == null || game_object.GetComponent< MinionIdentity >() != null || !game_object.GetComponent< Clearable >().isClearable )
                    continue;

                game_object.GetComponent< Clearable >().MarkForClear();
                Prioritizable component = game_object.GetComponent< Prioritizable >();
                if( component != null )
                    component.SetMasterPriority( ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority() );
            }

            if( !cSession.isHost() )
                return;

            cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }
    }
}