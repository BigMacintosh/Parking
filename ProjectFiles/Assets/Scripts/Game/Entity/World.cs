using System.Collections.Generic;
using System.Linq;
using Game.Core.Driving;
using Game.Core.Parking;
using Network;
using UnityEngine;
using Random = System.Random;

namespace Game.Entity {
    public abstract class World {
        // Private Fields
        
        public Dictionary<int, Player> Players { get; }

        protected World() {
            Players   = new Dictionary<int, Player>();
        }

        /// <summary>
        /// Creates a player, but does not spawn them
        /// Should be used when a new player connects
        /// </summary>
        /// <param name="playerID">The id of a player</param>
        /// <param name="playerOptions">The options set by a player</param>
        /// <param name="isControlledPlayer">Is the player belonging to the client creating it.</param>
        public void CreatePlayer(int playerID, PlayerOptions playerOptions, bool isControlledPlayer = false) {
            Players.Add(playerID, new Player(playerID, playerOptions, isControlledPlayer));
        }

        public void TEST_ONLY_AddPlayer(Player player) {
            Players.Add(player.PlayerID, player);
        }
        
        /// <summary>
        /// Destroys a players car.
        /// </summary>
        /// <param name="playerID">PlayerID of the player whose car should be destroyed.</param>
        public void DestroyPlayer(int playerID) {
            Players[playerID].DestroyCar();
            Players.Remove(playerID);
        }

        /// <summary>
        /// Gets the number of players connected.
        /// </summary>
        /// <returns>Number of players</returns>
        public ushort GetNumPlayers() {
            return (ushort) Players.Count;
        }

        /// <summary>
        /// Gets the playerIDs of players who are still in the game (not eliminated)
        /// </summary>
        /// <returns>List of playerIDs.</returns>
        public List<int> GetPlayersInGame() {
            return Players.Where(k=> !k.Value.IsEliminated)
                          .Select(k => k.Value.PlayerID)
                          .ToList();
        }

        /// <summary>
        /// Moves a player to the given position
        /// </summary>
        /// <param name="playerID">The ID of the player to be moved.</param>
        /// <param name="playerPosition">Where to move the player to.</param>
        public void MovePlayer(int playerID, PlayerPosition playerPosition) {
            Players[playerID].Move(playerPosition);
        }

        /// <summary>
        /// Gets the position of a player.
        /// </summary>
        /// <param name="playerID">ID of the player position to get.</param>
        /// <returns>Players position</returns>
        protected PlayerPosition GetPlayerPosition(int playerID) {
            return Players[playerID].GetPosition();
        }

        public void OnEliminatePlayers(ushort roundNumber, List<int> eliminatedPlayers) {
            foreach (var player in eliminatedPlayers) {
                Players[player].Eliminate();
            }
        }

        /// <summary>
        /// To reset the world at the end of the game.
        /// </summary>
        protected void Reset() {
            Players.Clear();
        }
    }
}