using System.Runtime.CompilerServices;

namespace MultiplayerNotIncluded.DebugTools
{
    public static class cLogger
    {
        private enum eLogLevel
        {
            kInfo,
            kWarning,
            kError,
        }

        public static void logInfo( string                    _message,
                                    [CallerMemberName] string _function = "",
                                    [CallerFilePath]   string _file     = "" )
        {
            log( eLogLevel.kInfo, _message, _function, _file );
        }

        public static void logWarning( string                    _message,
                                       [CallerMemberName] string _function = "",
                                       [CallerFilePath]   string _file     = "" )
        {
            log( eLogLevel.kWarning, _message, _function, _file );
        }

        public static void logError( string                    _message,
                                     [CallerMemberName] string _function = "",
                                     [CallerFilePath]   string _file     = "" )
        {
            log( eLogLevel.kError, _message, _function, _file );
        }

        private static void log( eLogLevel _level,
                                 string    _message,
                                 string    _function = "",
                                 string    _file     = "" )
        {
            string file_name         = System.IO.Path.GetFileName( _file );
            string formatted_message = $"[MNI:{file_name.Replace( ".cs", "" )}:{_function}] {_message}";

            switch( _level )
            {
                case eLogLevel.kInfo:    Debug.Log( formatted_message ); break;
                case eLogLevel.kWarning: Debug.LogWarning( formatted_message ); break;
                case eLogLevel.kError:   Debug.LogError( formatted_message ); break;
                default:                 Debug.Log( formatted_message ); break;
            }
        }
    }
}