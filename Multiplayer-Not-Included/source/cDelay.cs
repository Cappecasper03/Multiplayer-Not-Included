using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerNotIncluded
{
    public class cDelay
    {
        private CoroutineRunner m_runner;

        public void start( float _delay, UnityAction _action )
        {
            if( m_runner == null )
                m_runner = CoroutineRunner.Create();

            m_runner.StartCoroutine( coroutine( _delay, _action ) );
        }

        public void stopAndStart( float _delay, UnityAction _action )
        {
            stop();
            start( _delay, _action );
        }

        public void stop() => m_runner?.StopAllCoroutines();

        private static IEnumerator coroutine( float _delay, UnityAction _action )
        {
            float since_startup = Time.realtimeSinceStartup;

            while( Time.realtimeSinceStartup - since_startup < _delay )
                yield return null;

            _action?.Invoke();
        }
    }
}