using System;
using Unity.Collections;
using UnityEngine;

namespace Game.Entity {
    /// <summary>
    /// Stores all the options a player can set
    /// </summary>
    public struct PlayerOptions {
        // Public Fields
        public Color   CarColour  { get; set; }
        public CarType CarType    { get; set; }
        public string  PlayerName { get; set; }
    }

    /// <summary>
    /// Enum used to select a car type
    /// </summary>
    [Flags]
    public enum CarType {
        Hatchback = 0x01,
        Truck     = 0x02,
        Bike      = 0x03,

        // Space for more cars.

        PoliceCar = 0x10,
    }

    internal static class CarTypeMethods {
        private static readonly GameObject Hatchback = Resources.Load<GameObject>("Car");
        private static readonly GameObject Truck;
        private static readonly GameObject Bike;
        private static readonly GameObject PoliceCar = Resources.Load<GameObject>("Police");

        /// <summary>
        /// Allows you to get a prefab for a CarType.
        /// </summary>
        /// <param name="carType">The CarType you want a prefab for.</param>
        /// <returns>The prefab GameObject for the CarType.</returns>
        /// <exception cref="NotImplementedException">Thrown if the prefab does not exist for a CarType</exception>
        public static GameObject GetPrefab(this CarType carType) {
            switch (carType) {
                case CarType.Hatchback:
                    return Hatchback;
                case CarType.Truck:
                    throw new NotImplementedException("Prefab has not been created yet.");
                case CarType.Bike:
                    throw new NotImplementedException("Prefab has not been created yet.");
                case CarType.PoliceCar:
                    return PoliceCar;
                default:
                    throw new NotImplementedException($"CarType {carType} has not been added to GetPrefab Extension method.");
            }
        }
    }
}