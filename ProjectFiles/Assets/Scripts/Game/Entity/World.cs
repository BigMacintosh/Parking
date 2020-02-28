using System.Collections.Generic;
using System.Linq;
using Game.Core.Driving;
using Game.Core.Parking;
using Network;
using UnityEngine;
using Random = System.Random;

namespace Game.Entity {
    public class World {
        
        // Private Fields
        private readonly GameObject    carPrefab;
        private readonly PlayerSpawner spawner;

        public World(ParkingSpaceManager parkingSpaceManager) {
            carPrefab = Resources.Load<GameObject>("Car");
            Players   = new Dictionary<int, GameObject>();
            spawner   = new PlayerSpawner(parkingSpaceManager);
        }

        public Dictionary<int, GameObject> Players { get; }

        public void Update() {
            // Loop here and apply all network changes.

            // Interpolate players to new location.
        }

        public void SpawnPlayers(List<int> playersToSpawn) {
            foreach (var player in playersToSpawn) {
                SpawnPlayer(player);
            }
        }

        // Server one
        public void SpawnPlayer(int playerID) {
            var spawnPosition = spawner.GetSpawnPosition();
            SpawnPlayer(playerID, spawnPosition.position, spawnPosition.rotation);
        }

        // Client one
        public void SpawnPlayer(int playerID, Vector3 position, Quaternion rotation) {
            var newCar = Object.Instantiate(carPrefab, position, rotation);
            Players.Add(playerID, newCar);
        }


        public void SetPlayerControllable(int playerID) {
            Players[playerID].GetComponent<Vehicle>().SetControllable();
            ClientConfig.PlayerID = playerID;
        }

        public void DestroyPlayer(int playerID) {
            Object.Destroy(Players[playerID]);
            Players.Remove(playerID);
            if (playerID == ClientConfig.PlayerID) {
                ClientConfig.PlayerID = -1;
            }
        }

        public ushort GetNumPlayers() {
            return (ushort) Players.Count;
        }

        public List<int> GetPlayers() {
            return Players.Keys.ToList();
        }

        // TODO: Do we really want all this stuff below...?
        public Transform GetPlayerTransform(int playerID) {
            return Players[playerID].transform;
        }

        public Vector3 GetPlayerVelocity(int playerID) {
            return Players[playerID].GetComponent<Rigidbody>().velocity;
        }

        public Vector3 GetPlayerAngularVelocity(int playerID) {
            return Players[playerID].GetComponent<Rigidbody>().angularVelocity;
        }

        public void SetPlayerPosition(int playerID, Vector3 position) {
            if (!Players.ContainsKey(playerID)) return;

            Players[playerID].transform.position = position;
        }

        public void SetPlayerRotation(int playerID, Quaternion rotation) {
            if (!Players.ContainsKey(playerID)) return;

            Players[playerID].transform.rotation = rotation;
        }

        public void SetPlayerVelocity(int playerID, Vector3 velocity) {
            if (!Players.ContainsKey(playerID)) return;

            Players[playerID].GetComponent<Rigidbody>().velocity = velocity;
        }

        public void SetPlayerAngularVelocity(int playerID, Vector3 angularVelocity) {
            if (!Players.ContainsKey(playerID)) return;

            Players[playerID].GetComponent<Rigidbody>().angularVelocity = angularVelocity;
        }

        public bool PlayerExists(int playerID) {
            return Players.ContainsKey(playerID);
        }
    }

    public static class SpawnLocations {
        public static List<Vector3> Locations { get; set; }


        public static Vector3 GetSpawn() {
            var rand = new Random();
            return Locations[rand.Next(0, Locations.Count - 1)];
        }
    }
}