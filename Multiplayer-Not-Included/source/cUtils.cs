using UnityEngine;

namespace MultiplayerNotIncluded
{
    public static class cUtils
    {
        public static bool isInMenu() => App.GetCurrentSceneName() == "frontend";
        public static bool isInGame() => App.GetCurrentSceneName() == "backend";

        public static Vector2 getRegularizedPos( Vector2 _input, bool _minimize )
        {
            Vector3 size = new Vector3( Grid.HalfCellSizeInMeters, Grid.HalfCellSizeInMeters, 0 );
            return Grid.CellToPosCCC( Grid.PosToCell( _input ), Grid.SceneLayer.Background ) + ( _minimize ? -size : size );
        }
    }
}