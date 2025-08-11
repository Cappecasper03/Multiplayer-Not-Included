using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cCancelBuildPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private int      m_cell;
        private string   m_name;

        public ePacketType m_type => ePacketType.kCancelBuild;

        public cCancelBuildPacket() {}

        public cCancelBuildPacket( int _cell, string _name )
        {
            m_cell = _cell;
            m_name = _name;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_cell );
            _writer.Write( m_name );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_cell     = _reader.ReadInt32();
            m_name     = _reader.ReadString();
        }

        public void onReceived()
        {
            Constructable[] constructables = Object.FindObjectsOfType< Constructable >();
            foreach( Constructable constructable in constructables )
            {
                if( m_cell != Grid.PosToCell( constructable.transform.localPosition ) )
                    continue;

                cConstructablePatch.s_skip_send = true;
                Traverse.Create( constructable ).Method( "OnPressCancel" )?.GetValue();
                cConstructablePatch.s_skip_send = false;
                break;
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_cell}, {m_name}" );
    }
}