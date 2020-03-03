using System.Collections.Generic;
using Network;
using UnityEngine;

namespace Game.Entity {
    public class ClientWorld : World {

        /// <summary>
        /// For use in standalone mode. 
        /// </summary>
        public void SpawnPlayers() {
            var spawnPos = new PlayerPosition {
                Pos = new Vector3 {
                    x = -6.82f,
                    y = 2.19f,
                    z = -0.7f,
                },
                Rot = new Quaternion(),
            };
            foreach (var kv in Players) {
                kv.Value.Spawn(spawnPos);
            }
        }

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