using System;
using System.Collections.Generic;
using Network;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Game
{
    public class World
    {
        private readonly GameObject carPrefab;
        private readonly Dictionary<int, GameObject> cars;
        public Dictionary<int, GameObject> Players => cars;
        public int ClientID { get; set; }

        public World()
        {
            carPrefab = Resources.Load<GameObject>("Car");
            cars = new Dictionary<int, GameObject>();
            ClientID = -1;
        }

        

        public void Update()
        {
            // Loop here and apply all network changes.

            // Interpolate players to new location.
        }

        public void SpawnPlayers(List<int> playersToSpawn)
        {
            foreach (var player in playersToSpawn)
            {
                SpawnPlayer(player);
            }
        }

        // Server one
        public void SpawnPlayer(int playerID)
        {
            var position = SpawnLocations.GetSpawn();
            SpawnPlayer(playerID, position, Quaternion.identity);
        }

        // Client one
        public void SpawnPlayer(int playerID, Vector3 position, Quaternion rotation)
        {
            var newCar = Object.Instantiate(carPrefab, position, rotation);
            cars.Add(playerID, newCar);
        }
        
        public void SetPlayerControllable(int playerID)
        {
            cars[playerID].GetComponent<Vehicle.Vehicle>().SetControllable();
        }
        
        public void DestroyPlayer(int playerID)
        {
            Object.Destroy(cars[playerID]);
            cars.Remove(playerID);
        }
        
        public ushort GetNumPlayers()
        {
            return (ushort) cars.Count;
        }
        
        // TODO: Do we really want all this stuff below...?
        public Transform GetPlayerTransform(int playerID)
        {
            return cars[playerID].transform;
        }

        public Vector3 GetPlayerVelocity(int playerID)
        {
            return cars[playerID].GetComponent<Rigidbody>().velocity;
        }

        public Vector3 GetPlayerAngularVelocity(int playerID)
        {
            return cars[playerID].GetComponent<Rigidbody>().angularVelocity;
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

        public void SetPlayerVelocity(int playerID, Vector3 velocity)
        {
            if (!cars.ContainsKey(playerID)) return;

            cars[playerID].GetComponent<Rigidbody>().velocity = velocity;
        }

        public void SetPlayerAngularVelocity(int playerID, Vector3 angularVelocity)
        {
            if (!cars.ContainsKey(playerID)) return;

            cars[playerID].GetComponent<Rigidbody>().angularVelocity = angularVelocity;
        }

        public bool PlayerExists(int playerID)
        {
            return cars.ContainsKey(playerID);
        }

        
    }

    public static class SpawnLocations
    {
        public static List<Vector3> Locations { get; set; }


        public static Vector3 GetSpawn()
        {
            
            var rand = new Random();
            return Locations[rand.Next(0, Locations.Count - 1)];
        }
    }
    
    // public class SpawnLocations
    // {
    //     private readonly List<Vector3> locations = new List<Vector3>();
    //
    //     public SpawnLocations()
    //     {
    //         locations.Add(new Vector3(41.5f, 39.7f, 94.671f));
    //         locations.Add(new Vector3(41.5f, 39.7f, 130.0f));
    //         locations.Add(new Vector3(41.5f, 39.7f, 130.0f));
    //     }
    //
    //     public Vector3 GetSpawn()
    //     {
    //         
    //         
    //         var rand = new Random();
    //         return locations[rand.Next(0, locations.Count - 1)];
    //     }
    // }
}