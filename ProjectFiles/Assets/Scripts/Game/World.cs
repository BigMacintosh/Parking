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
        private List<DriveController> cars;
        private List<NetworkChange> networkChanges;
        private DriveController carPrefab;
        
        private class SpawnLocations
        {

            private List<Vector3> locations;

            public SpawnLocations()
            {
                locations.Add(new Vector3(41.5f, 39.7f, 94.671f));
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
            carPrefab = Resources.Load<DriveController>("Car");
            
            cars = new List<DriveController>();
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
        public Vector3 SpawnPlayer()
        {
            var position = new SpawnLocations().GetSpawn();
            var newCar = Object.Instantiate(carPrefab, position, Quaternion.identity);
            newCar.isControllable = false;
            cars.Add(newCar);
            return position;
        }
        
        // Client one
        public void SpawnPlayer(Vector3 position)
        {
            var newCar = Object.Instantiate(carPrefab, position, Quaternion.identity);
            newCar.isControllable = true;
            cars.Add(newCar);
        }
        
    }
}