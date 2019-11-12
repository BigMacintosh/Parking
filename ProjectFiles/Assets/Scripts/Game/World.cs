using System.Collections.Generic;
using Car;
using Network;
using UnityEngine;

namespace Game
{
    public class World
    {
        public List<DriveController> Cars { get; private set; }
        private Spawner spawner;
        private List<NetworkChange> networkChanges;
        private DriveController carPrefab;
        
        public World(Spawner spawner)
        {
            carPrefab = Resources.Load<DriveController>("Car");
            
            this.spawner = spawner;
            Cars = new List<DriveController>();
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
        
        
        public void SpawnPlayer()
        {
            
            Vector3 testPos = new Vector3(41.5f, 39.7f, 94.671f);
            
            var newCar = spawner.spawn(carPrefab, testPos, Quaternion.identity);
            newCar.isControllable = true;
            Cars.Add(newCar);
        }
    }
}