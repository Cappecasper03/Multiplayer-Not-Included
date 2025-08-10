using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Creatures;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.World.Creatures
{
    public class cAttackCreaturePacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_targeted;
        private int      m_instance_id;

        public ePacketType m_type => ePacketType.kAttackCreature;

        public cAttackCreaturePacket() {}

        public cAttackCreaturePacket( bool _targeted, int _instance_id )
        {
            m_targeted    = _targeted;
            m_instance_id = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_targeted );
            _writer.Write( m_instance_id );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_targeted    = _reader.ReadBoolean();
            m_instance_id = _reader.ReadInt32();
        }

        public void onReceived()
        {
            foreach( FactionAlignment faction_alignment in Components.FactionAlignments.Items )
            {
                if( faction_alignment.kprefabID.InstanceID != m_instance_id )
                    continue;

                cFactionAlignmentPatch.s_skip_send = true;
                faction_alignment.SetPlayerTargeted( m_targeted );
                cFactionAlignmentPatch.s_skip_send = false;

                Game.Instance.userMenu.Refresh( faction_alignment.gameObject );
                break;
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_targeted}, {m_instance_id}" );
    }
}