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
        private Spawner spawner;
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
        
        public World(Spawner spawner)
        {
            carPrefab = Resources.Load<DriveController>("Car");
            
            this.spawner = spawner;
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
        
        
        public Vector3 SpawnPlayer()
        {
            var position = new SpawnLocations().GetSpawn();
            
            // var newCar = spawner.spawn(carPrefab, position, Quaternion.identity);
            var newCar = Object.Instantiate(carPrefab, position, Quaternion.identity);
            newCar.isControllable = true;
            cars.Add(newCar);
            return position;
        }
        
        public void SpawnPlayer(Vector3 position)
        {
            var newCar = spawner.spawn(carPrefab, position, Quaternion.identity);
            newCar.isControllable = true;
            cars.Add(newCar);
        }
        
    }
}