using System.Collections.Generic;
using Steamworks;

namespace MultiplayerNotIncluded.Networking
{
    public static class MultiplayerSession
    {
        public static          bool                                      ShouldHostAfterLoad = false;
        public static readonly Dictionary< CSteamID, MultiplayerPlayer > ConnectedPlayers    = new Dictionary< CSteamID, MultiplayerPlayer >();
        public static          CSteamID                                  HostSteamID { get; private set; } = CSteamID.Nil;

        public static readonly Dictionary< CSteamID, PlayerCursor > PlayerCursors = new Dictionary< CSteamID, PlayerCursor >();

        public static void Clear()
        {
            ConnectedPlayers.Clear();
            HostSteamID = CSteamID.Nil;
            DebugTools.Logger.LogInfo( "Session cleared" );
        }

        public static void SetHost( CSteamID steamID )
        {
            HostSteamID = steamID;
            DebugTools.Logger.LogInfo( $"Host ID: {HostSteamID}" );
        }
    }
}