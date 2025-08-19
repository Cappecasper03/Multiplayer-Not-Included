using System.Collections.Generic;
using KSerialization;
using MultiplayerNotIncluded.DebugTools;

namespace MultiplayerNotIncluded.source.Networking.Components
{
    public class cNetworkIdentity : KMonoBehaviour
    {
        private static readonly Dictionary< int, cNetworkIdentity > s_identities = new Dictionary< int, cNetworkIdentity >();

        private static readonly Queue< int > s_free_ids = new Queue< int >();
        private static          int          s_next_id;

        [Serialize]
        private int m_network_id = -1;

        public static bool tryGetIdentity( int _id, out cNetworkIdentity _identity ) => s_identities.TryGetValue( _id, out _identity );

        public int getNetworkId() => m_network_id;

        public string getProperName()
        {
            MinionIdentity identity = GetComponent< MinionIdentity >();
            return identity != null ? identity.GetProperName() : "null";
        }

        public void set( int _id )
        {
            cNetworkIdentity identity;
            if( s_identities.TryGetValue( _id, out identity ) )
            {
                if( identity.gameObject && gameObject )
                    cLogger.logInfo( $"Overwriting {_id}: {identity.gameObject.name} => {gameObject.name}" );
                else
                    cLogger.logInfo( $"Overwriting {_id}" );
            }

            s_identities[ _id ] = this;
            cLogger.logInfo( gameObject ? $"{gameObject.name}: {m_network_id}" : $"{m_network_id}" );
        }

        public static void clear()
        {
            s_next_id = 0;
            s_free_ids.Clear();
            s_identities.Clear();
        }

        private void register()
        {
            m_network_id = getNextId();
            s_identities.Add( m_network_id, this );
            cLogger.logInfo( $"{gameObject.name}: {m_network_id}" );
        }

        private void unregister()
        {
            s_identities.Remove( m_network_id );
            s_free_ids.Enqueue( m_network_id );
            cLogger.logInfo( $"{gameObject.name}: {m_network_id}" );
        }

        private static int getNextId()
        {
            int id;
            do
            {
                id = s_free_ids.Count > 0 ? s_free_ids.Dequeue() : s_next_id++;
            } while( s_identities.ContainsKey( id ) );

            return id;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if( m_network_id < 0 )
                register();
            else
                set( m_network_id );
        }

        protected override void OnCleanUp()
        {
            unregister();
            base.OnCleanUp();
        }
    }
}