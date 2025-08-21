using System.Collections.Generic;
using System.IO;
using Database;
using Klei.AI;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cImmigrantPacket : iIPacket
    {
        private enum eAction
        {
            kSync,
            kSelect,
            kReject,
        }

        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private eAction  m_action;
        private int      m_cell;
        private int      m_layer;
        private string   m_selected_id;

        public static List< ITelepadDeliverable > s_deliverables = new List< ITelepadDeliverable >();

        public ePacketType m_type => ePacketType.kImmigrantScreen;

        public static cImmigrantPacket createSync( int _cell, int _layer, List< ITelepadDeliverable > _deliverables )
        {
            cImmigrantPacket packet = new cImmigrantPacket { m_action = eAction.kSync, m_cell = _cell, m_layer = _layer };
            s_deliverables = _deliverables;
            return packet;
        }

        public static cImmigrantPacket createSelect( int _cell, int _layer, string _selected_id )
        {
            return new cImmigrantPacket { m_action = eAction.kSelect, m_cell = _cell, m_layer = _layer, m_selected_id = _selected_id };
        }

        public static cImmigrantPacket createReject( int _cell, int _layer )
        {
            return new cImmigrantPacket { m_action = eAction.kReject, m_cell = _cell, m_layer = _layer };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_cell );
            _writer.Write( m_layer );

            switch( m_action )
            {
                case eAction.kSync:   serializeSync( _writer ); break;
                case eAction.kSelect: _writer.Write( m_selected_id ); break;
                case eAction.kReject: break;
            }
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_action   = ( eAction )_reader.ReadInt32();
            m_cell     = _reader.ReadInt32();
            m_layer    = _reader.ReadInt32();

            switch( m_action )
            {
                case eAction.kSync:   deserializeSync( _reader ); break;
                case eAction.kSelect: m_selected_id = _reader.ReadString(); break;
                case eAction.kReject: break;
            }
        }

        public void onReceived()
        {
            GameObject game_object = Grid.Objects[ m_cell, m_layer ];
            Telepad    telepad     = game_object?.GetComponent< Telepad >();
            if( telepad == null )
                return;

            switch( m_action )
            {
                case eAction.kSync:
                {
                    cImmigrantScreenPatch.s_skip_send = true;
                    cUtils.invokeMethod( ImmigrantScreen.instance, "Initialize", telepad );
                    cImmigrantScreenPatch.s_skip_send = false;
                    break;
                }
                case eAction.kSelect:
                {
                    Telepad current_telepad = cUtils.getField< Telepad >( ImmigrantScreen.instance, "telepad" );
                    if( current_telepad != null && current_telepad != telepad )
                        return;

                    ITelepadDeliverable deliverable = findDeliverable();
                    if( deliverable == null )
                        return;

                    cUtils.setField( ImmigrantScreen.instance, "selectedDeliverables", new List< ITelepadDeliverable > { deliverable } );

                    cImmigrantScreenPatch.s_skip_send = true;
                    cUtils.invokeMethod( ImmigrantScreen.instance, "OnProceed" );
                    cImmigrantScreenPatch.s_skip_send = false;

                    break;
                }

                case eAction.kReject:
                {
                    Telepad current_telepad = cUtils.getField< Telepad >( ImmigrantScreen.instance, "telepad" );
                    if( current_telepad != null && current_telepad != telepad )
                        return;

                    cImmigrantScreenPatch.s_skip_send = true;
                    cUtils.invokeMethod( ImmigrantScreen.instance, "OnRejectionConfirmed" );
                    cImmigrantScreenPatch.s_skip_send = false;

                    s_deliverables.Clear();
                    break;
                }
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kSync:   cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {( Grid.SceneLayer )m_layer}, {s_deliverables.Count}" ); break;
                case eAction.kSelect: cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {( Grid.SceneLayer )m_layer}, {m_selected_id}" ); break;
                case eAction.kReject: cLogger.logInfo( $"{_message}: {m_action}, {m_cell}, {( Grid.SceneLayer )m_layer}" ); break;
            }
        }

        private static void serializeSync( BinaryWriter _writer )
        {
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

        private static void deserializeSync( BinaryReader _reader )
        {
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

        private ITelepadDeliverable findDeliverable()
        {
            foreach( ITelepadDeliverable deliverable in s_deliverables )
            {
                switch( deliverable )
                {
                    case MinionStartingStats stats:
                    {
                        if( stats.personality.Id == m_selected_id )
                            return stats;
                        break;
                    }
                    case CarePackageInfo care_package:
                    {
                        if( care_package.id == m_selected_id )
                            return care_package;
                        break;
                    }
                }
            }

            List< ITelepadDeliverableContainer > containers = cUtils.getField< List< ITelepadDeliverableContainer > >( ImmigrantScreen.instance, "containers" );
            if( containers == null )
                return null;

            foreach( ITelepadDeliverableContainer container in containers )
            {
                switch( container )
                {
                    case CharacterContainer character:
                    {
                        if( character.Stats.personality.Id == m_selected_id )
                            return character.Stats;
                        break;
                    }
                    case CarePackageContainer care_package:
                    {
                        if( care_package.Info.id == m_selected_id )
                            return care_package.Info;
                        break;
                    }
                }
            }

            return null;
        }
    }
}