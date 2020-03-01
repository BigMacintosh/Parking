using System;
using System.Collections.Generic;
using Game.Core.Parking;
using UnityEngine;
using Random = System.Random;

namespace Game.Entity {
    /// <summary>
    /// Responsible for calculating all the players initial spawn locations.
    /// </summary>
    public class PlayerSpawner {
        private readonly List<Transform> availableSpawnLocations;
        private readonly Random          rand = new Random();

        public PlayerSpawner(ParkingSpaceManager parkingSpaceManager) {
            availableSpawnLocations = parkingSpaceManager.GetSpaceTransforms();
        }

        /// <summary>
        /// Gets a singular spawn location
        /// </summary>
        /// <returns>The transform for a spawn location</returns>
        /// <exception cref="NotEnoughSpacesException">If there are not enough spawn locations, this is thrown.</exception>
        public Transform GetSpawnPosition() {
            if (availableSpawnLocations.Count > 0) {
                var randomSpace = rand.Next(0, availableSpawnLocations.Count - 1);
                var position    = availableSpawnLocations[randomSpace];
                availableSpawnLocations.Remove(position);
                return position;
            }

            throw new NotEnoughSpacesException();
        }
    }

    /// <summary>
    /// Exception for when there are not enough spaces.
    /// </summary>
    public class NotEnoughSpacesException : Exception { }
}