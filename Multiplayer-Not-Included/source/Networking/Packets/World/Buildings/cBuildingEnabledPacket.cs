using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using MultiplayerNotIncluded.DebugTools;
using MultiplayerNotIncluded.Patches.World.Buildings;
using Steamworks;
using UnityEngine;

namespace MultiplayerNotIncluded.Networking.Packets.World.Buildings
{
    public class cBuildingEnabledPacket : iIPacket
    {
        private CSteamID m_steam_id = cSession.m_local_steam_id;
        private bool     m_enabled;
        private int      m_instance_id;

        public ePacketType m_type => ePacketType.kBuildingEnabled;

        public cBuildingEnabledPacket() {}

        public cBuildingEnabledPacket( bool _enabled, int _instance_id )
        {
            m_enabled     = _enabled;
            m_instance_id = _instance_id;
        }

        public void serialize( BinaryWriter _writer )
        {
            _writer.Write( m_steam_id.m_SteamID );
            _writer.Write( m_enabled );
            _writer.Write( m_instance_id );
        }

        public void deserialize( BinaryReader _reader )
        {
            m_steam_id    = new CSteamID( _reader.ReadUInt64() );
            m_enabled     = _reader.ReadBoolean();
            m_instance_id = _reader.ReadInt32();
        }

        public void onReceived()
        {
            BuildingEnabledButton[] buttons = Object.FindObjectsOfType< BuildingEnabledButton >();
            foreach( BuildingEnabledButton button in buttons )
            {
                KPrefabID prefab_id = button?.GetComponent< KPrefabID >();
                if( prefab_id == null || prefab_id.InstanceID != m_instance_id )
                    continue;

                bool queued_toggle = ( bool )AccessTools.Field( typeof( BuildingEnabledButton ), "queuedToggle" ).GetValue( button );

                cBuildingEnabledButtonPatch.s_skip_sending = true;
                if( m_enabled != queued_toggle )
                {
                    cLogger.logWarning( "Toggle object" );
                    MethodInfo allow_repair = button.GetType().GetMethod( "OnMenuToggle", BindingFlags.NonPublic | BindingFlags.Instance );
                    allow_repair?.Invoke( button, new object[] {} );
                }

                cBuildingEnabledButtonPatch.s_skip_sending = false;

                break;
            }

            if( cSession.isHost() )
                cPacketSender.sendToAllExcluding( this, new List< CSteamID > { m_steam_id } );
        }

        public void log( string _message ) => cLogger.logInfo( $"{_message}: {m_steam_id}" );
    }
}