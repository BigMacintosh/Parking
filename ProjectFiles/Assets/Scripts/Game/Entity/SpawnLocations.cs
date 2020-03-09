using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Game.Core.Parking;
using UnityEngine;
using Utils;
using Random = System.Random;

namespace Game.Entity {
    /// <summary>
    /// Responsible for calculating all the players initial spawn locations.
    /// </summary>
    public class SpawnLocations {
        private          List<Transform>     availableSpawnLocations;
        private readonly ParkingSpaceManager spaceManager;
        private readonly Random              rand = new Random();

        public SpawnLocations(ParkingSpaceManager parkingSpaceManager) {
            spaceManager = parkingSpaceManager;
            Reset();
        }

        /// <summary>
        /// Gets a singular spawn location
        /// </summary>
        /// <returns>The transform for a spawn location</returns>
        /// <exception cref="NotEnoughSpacesException">If there are not enough spawn locations, this is thrown.</exception>
        public PlayerPosition GetSpawnPosition() {
            if (availableSpawnLocations.Count > 0) {
                var randomSpace = rand.Next(0, availableSpawnLocations.Count - 1);
                var transform   = availableSpawnLocations[randomSpace];
                availableSpawnLocations.Remove(transform);
                return new PlayerPosition {
                    Transform = new ObjectTransform {
                        Position = transform.position,
                        Rotation = transform.rotation,
                    },
                };
            }

            throw new NotEnoughSpacesException();
        }

        public void Reset() {
            availableSpawnLocations = spaceManager.GetSpaceTransforms();
        }
    }

    /// <summary>
    /// Exception for when there are not enough spaces.
    /// </summary>
    [Serializable]
    public class NotEnoughSpacesException : Exception {
        public NotEnoughSpacesException() { }
        public NotEnoughSpacesException(string message) : base(message) { }
        public NotEnoughSpacesException(string message, Exception inner) : base(message, inner) { }

        protected NotEnoughSpacesException(
            SerializationInfo info,
            StreamingContext  context) : base(info, context) { }
    }
}