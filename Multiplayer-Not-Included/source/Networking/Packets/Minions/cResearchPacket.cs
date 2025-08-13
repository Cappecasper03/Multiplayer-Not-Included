using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.source.Patches.Minions;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.Minions
{
    public class cResearchPacket : iIPacket
    {
        private enum eAction
        {
            kStart,
            kCancel,
        }

        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private eAction  m_action;
        private string   m_name;

        public ePacketType m_type => ePacketType.kResearch;

        public static cResearchPacket createStart( string _name ) => new cResearchPacket { m_action = eAction.kStart, m_name = _name };

        public static cResearchPacket createCancel( string _name ) => new cResearchPacket { m_action = eAction.kCancel, m_name = _name };

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_name );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_action   = ( eAction )_reader.ReadInt32();
            m_name     = _reader.ReadString();
        }

        public void onReceived()
        {
            ResearchEntry entry;
            if( !cCacheManager.findAndCache( m_name, isEntry, out entry ) )
                return;

            cResearchEntryPatch.s_skip_send = true;
            switch( m_action )
            {
                case eAction.kStart:  Traverse.Create( entry ).Method( "OnResearchClicked" )?.GetValue(); break;
                case eAction.kCancel: Traverse.Create( entry ).Method( "OnResearchCanceled" )?.GetValue(); break;
            }

            cResearchEntryPatch.s_skip_send = false;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_action}, {m_name}" );

        private static bool isEntry( string _name, ResearchEntry _entry ) => _name == _entry.name;
    }
}