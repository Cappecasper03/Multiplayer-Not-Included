using System.IO;
using MultiplayerNotIncluded.Menus;

namespace MultiplayerNotIncluded.Saves
{
    public static class SaveHelper
    {
        public static string WorldName => Path.GetFileNameWithoutExtension( SaveLoader.GetActiveSaveFilePath() );

        public static byte[] GetWorldSave()
        {
            string path = SaveLoader.GetActiveSaveFilePath();
            SaveLoader.Instance.Save( path );
            return File.ReadAllBytes( path );
        }

        public static void LoadWorldSave( string name, byte[] data )
        {
            string saveDirectory = Path.Combine( Path.GetTempPath(), "MultiplayerNotIncluded" );
            string savePath      = Path.Combine( saveDirectory,      name );

            Directory.CreateDirectory( saveDirectory );

            var fileStream = new FileStream( savePath, FileMode.Create, FileAccess.Write, FileShare.None );
            var writer     = new BinaryWriter( fileStream );
            writer.Write( data );
            writer.Flush();
            writer.Close();

            MultiplayerLoadingOverlay.Show( "Loading..." );

            LoadScreen.DoLoad( savePath );
        }
    }
}