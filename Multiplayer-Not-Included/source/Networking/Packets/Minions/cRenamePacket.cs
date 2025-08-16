using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.source.Networking.Components;
using MultiplayerNotIncluded.source.Patches.Minions;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.Minions
{
    public class cRenamePacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private int      m_network_id;
        private string   m_name;

        public ePacketType m_type => ePacketType.kRename;

        public cRenamePacket() {}

        public cRenamePacket( int _network_id, string _name )
        {
            m_network_id = _network_id;
            m_name       = _name;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_network_id );
            _writer.Write( m_name );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id   = new CSteamID( _reader.ReadUInt64() );
            m_network_id = _reader.ReadInt32();
            m_name       = _reader.ReadString();
        }

        public void onReceived()
        {
            cNetworkIdentity identity;
            if( !cNetworkIdentity.tryGetIdentity( m_network_id, out identity ) )
                return;

            MinionIdentity minion_identity = identity.GetComponent< MinionIdentity >();
            if( minion_identity == null )
                return;

            cMinionIdentityPatch.s_skip_send = true;
            minion_identity.SetName( m_name );
            cMinionIdentityPatch.s_skip_send = false;

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_network_id}, {m_name}" );
    }
}