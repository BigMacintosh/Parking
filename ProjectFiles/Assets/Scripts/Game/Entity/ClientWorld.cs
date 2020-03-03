using System.Collections.Generic;
using Network;
using UnityEngine;

namespace Game.Entity {
    public class ClientWorld : World {

        /// <summary>
        /// Spawns all the players with the given locations
        /// To be used in standard client mode.
        /// </summary>
        /// <param name="playerPositions">A map of player ids to spawn positions</param>
        public void SpawnPlayers(Dictionary<int, PlayerPosition> playerPositions) {
            foreach (var kv in playerPositions) {
                Players[kv.Key].Spawn(playerPositions[kv.Key]);
            }
        }

        /// <summary>
        /// Gets the position of the player that is on the specific client
        /// </summary>
        /// <returns>Players Position</returns>
        public PlayerPosition GetMyPosition() {
            return GetPlayerPositon(ClientConfig.PlayerID);
        }
    }
}