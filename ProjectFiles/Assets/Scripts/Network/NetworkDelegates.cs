using System.Collections.Generic;

namespace Network
{
    public delegate void TriggerGameStartDelegate();
    
    public delegate void GameStartDelegate(ushort freeRoamLength, ushort nPlayers);
    
    public delegate void PreRoundStartDelegate(
        ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort nPlayers, List<ushort> spacesActive);
    
    public delegate void RoundStartDelegate(ushort roundNumber);
    
    public delegate void RoundEndDelegate(ushort roundNumber);
    
    public delegate void EliminatePlayersDelegate(ushort roundNumber, List<int> eliminatedPlayers);
    // Parking Space Delegates
    public delegate void SpaceEnterDelegate(int playerID, ushort spaceID);
    public delegate void SpaceExitDelegate(int playerID, ushort spaceID);
    public delegate void GameEndDelegate();
    public delegate void SpaceClaimedDelegate(int playerID, ushort spaceID);
    public delegate void PlayerCountChangeDelegate(ushort nPlayers);
}