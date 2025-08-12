using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;

namespace MultiplayerNotIncluded.Networking.Packets.Minions
{
    public class cConsumableInfoPacket : iIPacket
    {
        private CSteamID                 m_steam_id = cSession.m_local_steam_id;
        private TableRow.RowType         m_row_type;
        private string                   m_consumable_id;
        private TableScreen.ResultValues m_value;
        private string                   m_identity_name;

        public ePacketType m_type => ePacketType.kConsumableInfo;

        public cConsumableInfoPacket() {}

        public cConsumableInfoPacket( TableRow.RowType _row_type, string _consumable_id, TableScreen.ResultValues _value, string _identity_name )
        {
            m_row_type      = _row_type;
            m_consumable_id = _consumable_id;
            m_value         = _value;
            m_identity_name = _identity_name;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( ( int )m_row_type );
            _writer.Write( m_consumable_id );
            _writer.Write( ( int )m_value );

            if( m_row_type == TableRow.RowType.Minion )
                _writer.Write( m_identity_name );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id      = new CSteamID( _reader.ReadUInt64() );
            m_row_type      = ( TableRow.RowType )_reader.ReadInt32();
            m_consumable_id = _reader.ReadString();
            m_value         = ( TableScreen.ResultValues )_reader.ReadInt32();

            if( m_row_type == TableRow.RowType.Minion )
                m_identity_name = _reader.ReadString();
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
                    MinionIdentity identity;
                    if( !cCacheManager.findAndCache( m_identity_name, out identity ) )
                        return;

                    ConsumableConsumer component   = identity.GetComponent< ConsumableConsumer >();
                    bool               can_consume = m_value == TableScreen.ResultValues.True || m_value == TableScreen.ResultValues.ConditionalGroup;
                    component?.SetPermitted( m_consumable_id, can_consume );

                    break;
                }
            }

            Traverse.Create( ManagementMenu.Instance.consumablesScreen ).Method( "MarkRowsDirty" )?.GetValue();

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message )
        {
            if( m_row_type == TableRow.RowType.Minion )
                cLogger.logInfo( $"{_message}: {m_row_type}, {m_consumable_id}, {m_value}, {m_identity_name}" );
            else
                cLogger.logInfo( $"{_message}: {m_row_type}, {m_consumable_id}, {m_value}" );
        }
    }
}