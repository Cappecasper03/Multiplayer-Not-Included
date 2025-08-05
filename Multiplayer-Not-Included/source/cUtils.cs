using UnityEngine;

namespace MultiplayerNotIncluded
{
    public static class cUtils
    {
        public static bool isInMenu() => App.GetCurrentSceneName() == "frontend";
        public static bool isInGame() => App.GetCurrentSceneName() == "backend";
    }
}