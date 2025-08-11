using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.source.Patches.Minions.Skills;
using Steamworks;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded.Networking.Packets.Minions
{
    public class cSkillsPacket : iIPacket
    {
        private enum eAction
        {
            kHat,
            kSKill,
        }

        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private eAction  m_action;
        private string   m_identity;
        private string   m_hat_name;
        private string   m_hat;
        private string   m_skill;

        public ePacketType m_type => ePacketType.kSkills;

        public static cSkillsPacket createHat( string _identity, string _hat_name, string _hat )
        {
            return new cSkillsPacket { m_action = eAction.kHat, m_identity = _identity, m_hat_name = _hat_name, m_hat = _hat };
        }

        public static cSkillsPacket createSkill( string _identity, string _skill )
        {
            return new cSkillsPacket { m_action = eAction.kSKill, m_identity = _identity, m_skill = _skill };
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_action );
            _writer.Write( m_identity );

            switch( m_action )
            {
                case eAction.kHat:
                {
                    _writer.Write( m_hat_name );
                    _writer.Write( m_hat );
                    break;
                }
                case eAction.kSKill: _writer.Write( m_skill ); break;
            }
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id = new CSteamID( _reader.ReadUInt64() );
            m_action   = ( eAction )_reader.ReadInt32();
            m_identity = _reader.ReadString();

            switch( m_action )
            {
                case eAction.kHat:
                {
                    m_hat_name = _reader.ReadString();
                    m_hat      = _reader.ReadString();
                    break;
                }
                case eAction.kSKill: m_skill = _reader.ReadString(); break;
            }
        }

        public void onReceived()
        {
            MinionIdentity identity;
            if( !cUtils.findAndCache( m_identity, out identity ) )
                return;

            switch( m_action )
            {
                case eAction.kHat:
                {
                    HatListable hat = new HatListable( m_hat_name, m_hat );

                    SkillsScreen skill_screen = Object.FindObjectOfType< SkillsScreen >();
                    if( skill_screen == null )
                    {
                        MinionResume component = identity.GetComponent< MinionResume >();
                        component.SetHats( component.CurrentHat, hat.hat );
                    }
                    else
                    {
                        Traverse            traverse            = Traverse.Create( skill_screen );
                        IAssignableIdentity assignable_identity = traverse.Field( "currentlySelectedMinion" ).GetValue< IAssignableIdentity >();
                        if( assignable_identity == null )
                            return;

                        if( assignable_identity.GetProperName() != m_identity )
                        {
                            SkillMinionWidget[] widgets = Object.FindObjectsOfType< SkillMinionWidget >();
                            foreach( SkillMinionWidget widget in widgets )
                            {
                                if( widget.assignableIdentity.GetProperName() != m_identity )
                                    continue;

                                cSkillMinionWidgetPatch.s_skip_sending = true;
                                Traverse.Create( widget ).Method( "OnHatDropEntryClick", new[] { typeof( IListableOption ), typeof( object ) } ).GetValue( hat, null );
                                cSkillMinionWidgetPatch.s_skip_sending = false;
                                break;
                            }
                        }
                        else
                        {
                            cSkillsScreenPatch.s_skip_sending = true;
                            traverse.Method( "OnHatDropEntryClick", new[] { typeof( IListableOption ), typeof( object ) } ).GetValue( hat, null );
                            cSkillsScreenPatch.s_skip_sending = false;
                        }
                    }

                    break;
                }
                case eAction.kSKill:
                {
                    MinionResume component = identity.GetComponent< MinionResume >();
                    cMinionResumePatch.s_skip_sending = true;
                    component.MasterSkill( m_skill );
                    cMinionResumePatch.s_skip_sending = false;

                    SkillsScreen skill_screen = Object.FindObjectOfType< SkillsScreen >();
                    if( skill_screen != null )
                        skill_screen.RefreshAll();
                    break;
                }
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID >
                {
                    m_steam_id
                } );
        }

        public void log( string _message )
        {
            switch( m_action )
            {
                case eAction.kHat:   cLogger.logInfo( $"{_message}: {m_action}, {m_identity}, {m_hat_name}, {m_hat}" ); break;
                case eAction.kSKill: cLogger.logInfo( $"{_message}: {m_action}, {m_identity}, {m_skill}" ); break;
            }
        }
    }
}