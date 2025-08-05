using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.Tools
{
    public class cDigToolPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private int      m_cell;
        private int      m_animation_delay;

        public ePacketType m_type => ePacketType.kDigTool;

        public cDigToolPacket() {}

        public cDigToolPacket( int _cell, int _animation_delay )
        {
            m_cell            = _cell;
            m_animation_delay = _animation_delay;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_cell );
            _writer.Write( m_animation_delay );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id        = new CSteamID( _reader.ReadUInt64() );
            m_cell            = _reader.ReadInt32();
            m_animation_delay = _reader.ReadInt32();
        }

        public void onDispatched()
        {
            Vector3 position = Grid.CellToPosCBC( m_cell, DigTool.Instance.visualizerLayer );
            position.z -= .15f;

            GameObject dig_placer = Util.KInstantiate( Assets.GetPrefab( new Tag( "DigPlacer" ) ) );
            dig_placer.SetActive( true );

            dig_placer.transform.SetPosition( position );
            dig_placer.GetComponentInChildren< EasingAnimations >().PlayAnimation( "ScaleUp", Mathf.Max( 0, m_animation_delay * .02f ) );

            if( !cSession.isHost() )
                return;

            cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }
    }
}