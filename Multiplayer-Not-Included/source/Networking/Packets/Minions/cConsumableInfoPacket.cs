using System;
using System.Collections.Generic;
using System.IO;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;
using Object = UnityEngine.Object;

namespace MultiplayerNotIncluded.Networking.Packets.Minions
{
    public class cConsumableInfoPacket : iIPacket
    {
        private CSteamID                 m_steam_id = cSession.m_local_steam_id;
        private TableRow.RowType         m_row_type;
        private string                   m_identity_name;
        private string                   m_consumable_id;
        private TableScreen.ResultValues m_value;

        public ePacketType m_type => ePacketType.kConsumableInfo;

        public cConsumableInfoPacket() {}

        public cConsumableInfoPacket( TableRow.RowType _row_type, string _identity_name, string _consumable_id, TableScreen.ResultValues _value )
        {
            m_row_type      = _row_type;
            m_identity_name = _identity_name;
            m_consumable_id = _consumable_id;
            m_value         = _value;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_row_type );
            _writer.Write( m_identity_name );
            _writer.Write( m_consumable_id );
            _writer.Write( ( int )m_value );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id      = new CSteamID( _reader.ReadUInt64() );
            m_row_type      = ( TableRow.RowType )_reader.ReadInt32();
            m_identity_name = _reader.ReadString();
            m_consumable_id = _reader.ReadString();
            m_value         = ( TableScreen.ResultValues )_reader.ReadInt32();
        }

        public void onReceived()
        {
            switch( m_row_type )
            {
                case TableRow.RowType.Default:
                {
                    if( m_value == TableScreen.ResultValues.True )
                        ConsumerManager.instance.DefaultForbiddenTagsList.Remove( m_consumable_id.ToTag() );
                    else
                        ConsumerManager.instance.DefaultForbiddenTagsList.Add( m_consumable_id.ToTag() );
                    break;
                }
                case TableRow.RowType.Minion:
                {
                    MinionIdentity[] minion_identities = Object.FindObjectsOfType< MinionIdentity >();
                    foreach( MinionIdentity identity in minion_identities )
                    {
                        cLogger.logWarning( identity.GetProperName() );
                        if( identity.GetProperName() != m_identity_name )
                            continue;

                        ConsumableConsumer component   = identity.GetComponent< ConsumableConsumer >();
                        bool               can_consume = m_value == TableScreen.ResultValues.True || m_value == TableScreen.ResultValues.ConditionalGroup;
                        component.SetPermitted( m_consumable_id, can_consume );
                        break;
                    }

                    break;
                }
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_row_type}, {m_identity_name}, {m_consumable_id}, {m_value}" );
    }
}