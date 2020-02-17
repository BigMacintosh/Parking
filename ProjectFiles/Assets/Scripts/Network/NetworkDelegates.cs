using System.Collections.Generic;

namespace Network
{
    public delegate void GameStartDelegate(ushort nPlayers);
    
    public delegate void PreRoundStartDelegate(
        ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort nPlayers, List<ushort> spacesActive);
    
    public delegate void RoundStartDelegate(ushort roundNumber);
    
    public delegate void RoundEndDelegate(ushort roundNumber);
    
    public delegate void EliminatePlayersDelegate(ushort roundNumber, List<ushort> eliminatedPlayers);
}