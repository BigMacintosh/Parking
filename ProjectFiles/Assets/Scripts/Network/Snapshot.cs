using System.Collections.Generic;
using Game.Entity;
using System;

namespace Network {
    public struct GameSnapshot {
        public Dictionary<ushort, PlayerPosition> playerPositions;
        
        public static GameSnapshot NewGameSnapshot() {
            return new GameSnapshot {
                playerPositions = new Dictionary<ushort, PlayerPosition>()
            };
        }
    }
}