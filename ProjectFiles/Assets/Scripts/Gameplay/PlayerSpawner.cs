using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;
using Random = System.Random;

namespace Gameplay
{
    public class PlayerSpawner
    {
        private List<Transform> availableSpawnLocations;
        private Random rand = new Random();

        public PlayerSpawner(ParkingSpaceManager parkingSpaceManager)
        {
            availableSpawnLocations = parkingSpaceManager.GetSpaceTransforms();
        }

        public Transform GetSpawnPosition()
        {
            if (availableSpawnLocations.Count > 0)
            {
                int randomSpace = rand.Next(0, availableSpawnLocations.Count - 1);
                var position = availableSpawnLocations[randomSpace];
                availableSpawnLocations.Remove(position);
                return position;
            }
            
            throw new NotEnoughSpacesException();
        }
    }

    public class NotEnoughSpacesException : Exception
    {
        
    }
}
