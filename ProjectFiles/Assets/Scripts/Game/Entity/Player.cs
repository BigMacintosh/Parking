using System;
using System.Runtime.Serialization;
using Game.Core.Driving;
using Network;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace Game.Entity {
    /// <summary>
    /// Used for storing all details about a player
    /// </summary>
    public class Player {
        // Public Fields
        public int           PlayerID      { get; }
        public bool          IsEliminated  { get; private set; }
        public PlayerOptions PlayerOptions => playerOptions;
        public bool          isSpawned     { get; private set; }

        // Private Fields
        private GameObject    car; // Do not directly set this. Use SetCar method
        private VehicleDriver vehicleDriver;
        private Rigidbody     carRb;
        private Transform     carTrans;
        private PlayerOptions playerOptions;

        private readonly bool isMe;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="playerID">The network ID for the player</param>
        /// <param name="options">The options used to configure the player</param>
        /// <param name="me">Optional. Used to set the player to controllable.</param>
        public Player(int playerID, PlayerOptions options, bool me = false) {
            PlayerID      = playerID;
            playerOptions = options;
            isMe          = me;
            if (isMe) {
                ClientConfig.Me = this;
            }
        }

        public void ApplyInputs(VehicleInputState inputs) {
            if (!isSpawned) return;
            vehicleDriver.ApplyInputs(inputs);
        }

        /// <summary>
        /// Used to set the car object for a player.
        ///
        /// Used rather than setting to car directly to allow for the rigid body to be extracted.
        /// </summary>
        /// <param name="newCar"></param>
        private void SetCar(GameObject newCar) {
            car           = newCar;
            carRb         = newCar.GetComponent<Rigidbody>();
            carTrans      = newCar.transform;
        }

        /// <summary>
        /// The spawn method used by the server. 
        /// </summary>
        /// <param name="spawner">Allows the player to get a new spawn location</param>
        public void Spawn(SpawnLocations spawner) {
            var spawnPosition = spawner.GetSpawnPosition();
            Spawn(spawnPosition);
        }

        /// <summary>
        /// The spawn method used to spawn a player in a certain position
        /// </summary>
        public void Spawn(PlayerPosition spawnPosition) {
            if (isSpawned) throw new PlayerAlreadySpawnedException();
            var prefab = PlayerOptions.CarType.GetPrefab();
            Debug.Log($"Spawning at {spawnPosition.Transform.Position} with rot: {spawnPosition.Transform.Rotation}");
            SetCar(Object.Instantiate(prefab, spawnPosition.Transform.Position, spawnPosition.Transform.Rotation));

            // Set car to controllable if spawning this players car
            if (isMe) {
                car.GetComponent<Vehicle>().SetControllable();
                vehicleDriver = car.GetComponent<VehicleDriver>();
            }

            // Set the car colour
            SetCarColour();

            // Set player to spawned
            isSpawned = true;
        }

        /// <summary>
        /// Used to move a player.
        /// </summary>
        /// <param name="playerPosition">The position to move a player to</param>
        /// <exception cref="PlayerNotSpawnedException">Thrown if you try to move a player who hasn't been spawned</exception>
        public void Move(PlayerPosition playerPosition) {
            if (!isSpawned) throw new PlayerNotSpawnedException();
            carTrans.position     = playerPosition.Transform.Position;
            carTrans.rotation     = playerPosition.Transform.Rotation;
            carRb.velocity        = playerPosition.Velocity;
            carRb.angularVelocity = playerPosition.AngularVelocity;
        }

        /// <summary>
        /// Destroys a player's car
        /// </summary>
        /// <exception cref="PlayerNotSpawnedException">Throws if the car wasn't already created.</exception>
        public void DestroyCar() {
            if (!isSpawned) throw new PlayerNotSpawnedException();
            Object.Destroy(car);
        }

        /// <summary>
        /// Swaps a player to a new type of car.
        ///
        /// Usually used when swapping player to police car.
        /// </summary>
        /// <param name="newCarType"></param>
        private void SwapCar(CarType newCarType) {
            playerOptions.CarType = newCarType;

            if (!isSpawned) return;
            var previousPosition = GetPosition();
            DestroyCar();
            Spawn(previousPosition);
        }

        /// <summary>
        /// Sets the car colour to the value in playerOptions
        /// </summary>
        private void SetCarColour() {
            car.GetComponent<Vehicle>().SetBodyColour(playerOptions.CarColour);
        }

        /// <summary>
        /// Gets the position of this player.
        /// </summary>
        /// <returns>The position</returns>
        public PlayerPosition GetPosition() {
            return new PlayerPosition {
                Transform = new ObjectTransform {
                    Position = carTrans.position,
                    Rotation = carTrans.rotation
                },
                Velocity        = carRb.velocity,
                AngularVelocity = carRb.angularVelocity
            };
        }

        /// <summary>
        /// Eliminates a player from a game and changes their car to a police car.
        /// </summary>
        public void Eliminate() {
            IsEliminated = true;
            SwapCar(CarType.PoliceCar);
        }
    }


    /// <summary>
    /// Exception for when trying to spawn a player who has already been spawned
    /// </summary>
    [Serializable]
    public class PlayerAlreadySpawnedException : Exception {
        public PlayerAlreadySpawnedException() { }
        public PlayerAlreadySpawnedException(string message) : base(message) { }
        public PlayerAlreadySpawnedException(string message, Exception inner) : base(message, inner) { }

        protected PlayerAlreadySpawnedException(
            SerializationInfo info,
            StreamingContext  context) : base(info, context) { }
    }


    /// <summary>
    /// Exception for when trying to modify a player who hasn't been spawned
    /// </summary>
    [Serializable]
    public class PlayerNotSpawnedException : Exception {
        public PlayerNotSpawnedException() { }
        public PlayerNotSpawnedException(string message) : base(message) { }
        public PlayerNotSpawnedException(string message, Exception inner) : base(message, inner) { }

        protected PlayerNotSpawnedException(
            SerializationInfo info,
            StreamingContext  context) : base(info, context) { }
    }
}