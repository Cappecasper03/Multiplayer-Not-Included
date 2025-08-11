using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cCopySettingsToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private int      m_cell;
        private int      m_instance_id;

        public ePacketType m_type => ePacketType.kCopySettingsTool;

        public cCopySettingsToolPacket() {}

        public cCopySettingsToolPacket( int _cell, int _instance_id )
        {
            m_cell        = _cell;
            m_instance_id = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_cell );
            _writer.Write( m_instance_id );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_cell        = _reader.ReadInt32();
            m_instance_id = _reader.ReadInt32();
        }

        public void onReceived()
        {
            GameObject[] game_objects = Object.FindObjectsOfType< GameObject >();
            foreach( GameObject game_object in game_objects )
            {
                KPrefabID prefab_id = game_object.GetComponent< KPrefabID >();
                if( prefab_id == null || prefab_id.InstanceID != m_instance_id )
                    continue;

                CopyBuildingSettings.ApplyCopy( m_cell, game_object );

                Game.Instance.userMenu.Refresh( game_object.gameObject );
                break;
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_cell}, {m_instance_id}" );
    }
}