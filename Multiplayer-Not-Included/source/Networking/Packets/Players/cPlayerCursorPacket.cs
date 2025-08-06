using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.source.Networking.Components;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Players
{
    public class cPlayerCursorPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private Vector3  m_position;

        public ePacketType m_type => ePacketType.kPlayerCursor;

        public cPlayerCursorPacket() {}

        public cPlayerCursorPacket( Vector3 _position ) => m_position = _position;

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_position );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_position = _reader.ReadVector3();
        }

        public void onReceived()
        {
            if( !cUtils.isInGame() )
                return;

            cPlayer player;
            if( !cSession.tryGetPlayer( m_steam_id, out player ) )
                return;

            cPlayerCursorComponent cursor;
            if( player.getOrCreateCursor( out cursor ) )
            {
                cursor.m_target_position = m_position;
                cursor.setVisibility( true );
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) {}
    }
}