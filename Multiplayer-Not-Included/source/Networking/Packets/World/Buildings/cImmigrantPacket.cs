using System.Collections.Generic;
using System.IO;
using Database;
using Klei.AI;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cImmigrantPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;

        public static List< ITelepadDeliverable > s_deliverables = new List< ITelepadDeliverable >();

        public ePacketType m_type => ePacketType.kImmigrantScreen;

        public cImmigrantPacket() {}

        public cImmigrantPacket( List< ITelepadDeliverable > _deliverables )
        {
            s_deliverables = _deliverables;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );

            _writer.Write( s_deliverables.Count );
            foreach( ITelepadDeliverable deliverable in s_deliverables )
            {
                switch( deliverable )
                {
                    case MinionStartingStats stats:
                    {
                        _writer.Write( true );

                        _writer.Write( stats.personality.Id );
                        _writer.Write( stats.voiceIdx );
                        _writer.Write( stats.stickerType );

                        _writer.Write( stats.Traits.Count );
                        foreach( Trait trait in stats.Traits )
                            _writer.Write( trait.Id );

                        _writer.Write( stats.StartingLevels.Count );
                        foreach( KeyValuePair< string, int > pair in stats.StartingLevels )
                        {
                            _writer.Write( pair.Key );
                            _writer.Write( pair.Value );
                        }

                        _writer.Write( stats.skillAptitudes.Count );
                        foreach( KeyValuePair< SkillGroup, float > pair in stats.skillAptitudes )
                        {
                            _writer.Write( pair.Key.Id );
                            _writer.Write( pair.Value );
                        }

                        break;
                    }
                    case CarePackageInfo care_package:
                    {
                        _writer.Write( false );

                        _writer.Write( care_package.id );
                        _writer.Write( care_package.quantity );
                        _writer.Write( care_package.facadeID ?? "" );
                        break;
                    }
                }
            }
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            s_deliverables.Clear();

            int count = _reader.ReadInt32();
            for( int i = 0; i < count; i++ )
            {
                if( _reader.ReadBoolean() )
                {
                    string      personality_id = _reader.ReadString();
                    Personality personality    = Db.Get().Personalities.Get( personality_id );
                    MinionStartingStats stats = new MinionStartingStats( personality )
                    {
                        voiceIdx    = _reader.ReadInt32(),
                        stickerType = _reader.ReadString(),
                        IsValid     = true,
                    };

                    stats.Traits.Clear();
                    int trait_count = _reader.ReadInt32();
                    for( int j = 0; j < trait_count; j++ )
                        stats.Traits.Add( Db.Get().traits.Get( _reader.ReadString() ) );

                    stats.StartingLevels.Clear();
                    int level_count = _reader.ReadInt32();
                    for( int j = 0; j < level_count; j++ )
                        stats.StartingLevels.Add( _reader.ReadString(), _reader.ReadInt32() );

                    stats.skillAptitudes.Clear();
                    int aptitude_count = _reader.ReadInt32();
                    for( int j = 0; j < aptitude_count; j++ )
                        stats.skillAptitudes.Add( Db.Get().SkillGroups.Get( _reader.ReadString() ), _reader.ReadSingle() );

                    s_deliverables.Add( stats );
                }
                else
                {
                    CarePackageInfo care_package = new CarePackageInfo( _reader.ReadString(), _reader.ReadSingle(), null, _reader.ReadString() );

                    s_deliverables.Add( care_package );
                }
            }
        }

        public void onReceived()
        {
            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {s_deliverables.Count}" );
    }
}