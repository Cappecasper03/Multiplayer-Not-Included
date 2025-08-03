using System.Runtime.CompilerServices;

namespace MultiplayerNotIncluded.DebugTools
{
    public class Logger
    {
        private enum LogLevel
        {
            Info,
            Warning,
            Error,
        }

        public static void LogInfo( string                    message,
                                    [CallerMemberName] string function = "",
                                    [CallerFilePath]   string file     = "" )
        {
            Log( LogLevel.Info, message, function, file );
        }

        public static void LogWarning( string                    message,
                                       [CallerMemberName] string function = "",
                                       [CallerFilePath]   string file     = "" )
        {
            Log( LogLevel.Warning, message, function, file );
        }

        public static void LogError( string                    message,
                                     [CallerMemberName] string function = "",
                                     [CallerFilePath]   string file     = "" )
        {
            Log( LogLevel.Error, message, function, file );
        }

        private static void Log( LogLevel level,
                                 string   message,
                                 string   function = "",
                                 string   file     = "" )
        {
            string fileName         = System.IO.Path.GetFileName( file );
            string formattedMessage = $"[MNI:{fileName.Replace( ".cs", "" )}:{function}] {message}";

            switch( level )
            {
                case LogLevel.Info:    Debug.Log( formattedMessage ); break;
                case LogLevel.Warning: Debug.LogWarning( formattedMessage ); break;
                case LogLevel.Error:   Debug.LogError( formattedMessage ); break;
                default:               Debug.Log( formattedMessage ); break;
            }
        }
    }
}