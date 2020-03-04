using System.Collections.Generic;
using System.Linq;
using Game.Core.Parking;

namespace Game.Entity {
    public class ServerWorld : World{
        // Private Fields
        private readonly SpawnLocations spawner;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parkingSpaceManager">The spaceManager used by the server</param>
        public ServerWorld(ParkingSpaceManager parkingSpaceManager){
            spawner = new SpawnLocations(parkingSpaceManager);
        }
        
        /// <summary>
        /// Spawns all the players using the spawner.
        /// </summary>
        public void SpawnPlayers() {
            foreach (var kv in Players) {
                kv.Value.Spawn(spawner);
            }
        }

        public Dictionary<int, PlayerOptions> GetAllPlayerOptions() {
            return Players.ToDictionary(
                p => p.Key,
                p => p.Value.PlayerOptions);
        }
    }
}