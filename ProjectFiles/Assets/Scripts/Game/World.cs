using System.Collections.Generic;
using Car;
using Network;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

namespace Game
{
    
    public class World
    {
        private Dictionary<int, GameObject> cars;
        private int nextPlayerID = 0;
        private List<NetworkChange> networkChanges;
        private GameObject carPrefab;
        public int ClientID { get; set; }
        
        private class SpawnLocations
        {

            private List<Vector3> locations = new List<Vector3>();

            public SpawnLocations()
            {
                locations.Add(new Vector3(41.5f, 39.7f, 94.671f));
                locations.Add(new Vector3(41.5f, 39.7f, 130.0f));
                locations.Add(new Vector3(41.5f, 39.7f, 130.0f));
            }

            public Vector3 GetSpawn()
            {
                Random rand = new Random();
                return locations[rand.Next(0, locations.Count - 1)];
            }
        }
        
        public World()
        {
            carPrefab = Resources.Load<GameObject>("Car");
            cars = new Dictionary<int, GameObject>();
            ClientID = -1;
        }

        public void AddNetworkChange(NetworkChange networkChange)
        {
            networkChanges.Add(networkChange);
        }
        
        public void Update()
        {
            // Loop here and apply all network changes.
            
            // Interpolate players to new location.
        }

        // Server one
        public void SpawnPlayer(int playerID)
        {
            var position = new SpawnLocations().GetSpawn();
            SpawnPlayer(playerID, position, false);
        }
        
        // Client one
        public void SpawnPlayer(int playerID, Vector3 position, bool isControllable)
        {
            var newCar = Object.Instantiate(carPrefab, position, Quaternion.identity);
            cars.Add(playerID, newCar);
            if (isControllable)
            {
                SetPlayerControllable(playerID);
            }
        }

        public Transform GetPlayerTransform(int playerID)
        {
            return cars[playerID].transform;
        }

        public void SetPlayerControllable(int playerID)
        {
            cars[playerID].GetComponent<DriveController>().SetControllable();
        }

        public void SetPlayerPosition(int playerID, Vector3 position)
        {
            if (!cars.ContainsKey(playerID)) return;
            
            cars[playerID].transform.position = position;
        }

        public void SetPlayerRotation(int playerID, Quaternion rotation)
        {
            if (!cars.ContainsKey(playerID)) return;

            cars[playerID].transform.rotation = rotation;
        }

        public void DestroyPlayer(int playerID)
        {
            Object.Destroy(cars[playerID]);
            cars.Remove(playerID);
        }

        public bool PlayerExists(int playerID)
        {
            return cars.ContainsKey(playerID);
        }

        public int GetNumPlayers()
        {
            return cars.Count;
        }

        public IEnumerable<int> PlayerIDs => cars.Keys;
    }
}