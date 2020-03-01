using System.Collections.Generic;

namespace Network {
    // UI Delegates
    public delegate void TriggerGameStartDelegate();

    public delegate void PlayerCountChangeDelegate(ushort nPlayers);

    // Round Delegates
    public delegate void GameStartDelegate(ushort freeRoamLength, ushort nPlayers);

    public delegate void PreRoundStartDelegate(
        ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort nPlayers);

    public delegate void RoundStartDelegate(ushort roundNumber, List<ushort> spacesActive);

    public delegate void RoundEndDelegate(ushort roundNumber);

    public delegate void EliminatePlayersDelegate(ushort roundNumber, List<int> eliminatedPlayers);

    public delegate void GameEndDelegate(List<int> winners);

    // Space Delegates
    public delegate void SpaceEnterDelegate(int playerID, ushort spaceID);

    public delegate void SpaceExitDelegate(int playerID, ushort spaceID);

    public delegate void SpaceClaimedDelegate(int playerID, ushort spaceID);
}