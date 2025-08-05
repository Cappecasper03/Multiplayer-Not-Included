using System.IO;
using MultiplayerNotIncluded.Menus;
using MultiplayerNotIncluded.Networking;

namespace MultiplayerNotIncluded.Saves
{
    public static class cSaveHelper
    {
        public static string worldName => Path.GetFileNameWithoutExtension( SaveLoader.GetActiveSaveFilePath() );

        public static byte[] getWorldSave()
        {
            string path = SaveLoader.GetActiveSaveFilePath();
            SaveLoader.Instance.Save( path );
            return File.ReadAllBytes( path );
        }

        public static void loadWorldSave( string _name, byte[] _data )
        {
            string save_directory = Path.Combine( Path.GetTempPath(), "MultiplayerNotIncluded" );
            string save_path      = Path.Combine( save_directory,     _name );

            Directory.CreateDirectory( save_directory );

            var file_stream = new FileStream( save_path, FileMode.Create, FileAccess.Write, FileShare.None );
            var writer      = new BinaryWriter( file_stream );
            writer.Write( _data );
            writer.Flush();
            writer.Close();

            cGameClient.m_state = cGameClient.eClientState.kLoadingWorld;
            cMultiplayerLoadingOverlay.show( "Loading..." );

            LoadScreen.DoLoad( save_path );
        }
    }
}