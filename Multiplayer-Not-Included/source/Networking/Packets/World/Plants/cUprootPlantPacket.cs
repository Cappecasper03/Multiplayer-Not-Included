using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Plants
{
    public class cUprootPlantPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_marked;
        private int      m_instance_id;

        public ePacketType m_type => ePacketType.kUprootPlant;

        public cUprootPlantPacket() {}

        public cUprootPlantPacket( bool _marked, int _instance_id )
        {
            m_marked      = _marked;
            m_instance_id = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_marked );
            _writer.Write( m_instance_id );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_marked      = _reader.ReadBoolean();
            m_instance_id = _reader.ReadInt32();
        }

        public void onReceived()
        {
            Uprootable[] uprootables = Object.FindObjectsOfType< Uprootable >();
            foreach( Uprootable uprootable in uprootables )
            {
                KPrefabID prefab_id = uprootable.GetComponent< KPrefabID >();
                if( prefab_id == null || prefab_id.InstanceID != m_instance_id )
                    continue;

                if( m_marked )
                    uprootable.MarkForUproot( false );
                else
                    Traverse.Create( uprootable ).Method( "OnCancel", new[] { typeof( object ) } )?.GetValue( ( object )null );

                Game.Instance.userMenu.Refresh( uprootable.gameObject );
                break;
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_marked}, {m_instance_id}" );
    }
}