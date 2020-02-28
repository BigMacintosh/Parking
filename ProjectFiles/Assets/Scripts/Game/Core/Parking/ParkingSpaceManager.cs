using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Network;
using UI;
using UnityEngine;
using Utils;

namespace Game.Core.Parking {
    
    /// <summary>
    /// Generic ParkingSpaceManager
    /// Used to control all the parking spaces in the game.
    /// </summary>
    public class ParkingSpaceManager {
        // Delegates
        public event SpaceStateChangeDelegate SpaceStateChangeEvent;

        // Public fields
        public Dictionary<int, ParkingSpace> ParkingSpacesByPlayerID { get; private set; }

        // Protected fields
        protected Dictionary<ushort, ParkingSpace> parkingSpacesBySpaceID;

        /// <summary>
        ///  Constructor for ParkingSpaceManager
        /// </summary>
        protected ParkingSpaceManager() {
            parkingSpacesBySpaceID  = new Dictionary<ushort, ParkingSpace>();
            ParkingSpacesByPlayerID = new Dictionary<int, ParkingSpace>();

            var parkingSpaceList = Object.FindObjectsOfType<ParkingSpace>();
            foreach (var parkingSpace in parkingSpaceList) {
                parkingSpacesBySpaceID.Add(parkingSpace.SpaceID, parkingSpace);
            }

            DisableAllSpaces();
        }

        /// <summary>
        /// Disables all parking spaces.
        /// Used at the end of a round.
        /// </summary>
        public void DisableAllSpaces() {
            foreach (var space in parkingSpacesBySpaceID.Values) {
                space.Disable();
            }

            ParkingSpacesByPlayerID.Clear();
        }

        /// <summary>
        /// Enables a subset of all the possible parking spaces
        /// Used at the start of a round.
        /// </summary>
        /// <param name="spaces">The spaces to be activated.</param>
        public void EnableSpaces(List<ushort> spaces) {
            foreach (var space in spaces) {
                parkingSpacesBySpaceID[space].Enable();
            }
        }

        /// <summary>
        /// Event handler for when a space is claimed.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="spaceID"></param>
        public void OnSpaceClaimed(int playerID, ushort spaceID) {
            var parkingSpace = parkingSpacesBySpaceID[spaceID];
            if (parkingSpace.Occupied()) {
                Debug.Log(playerID + " stole a space from " + parkingSpace.OccupiedBy);
                if (parkingSpace.OccupiedBy == ClientConfig.PlayerID) {
                    // Tell the UI "parkingSpace.OccupiedBy" had their space stolen.
                    SpaceStateChangeEvent?.Invoke(SpaceState.StolenLost, spaceID);
                } else if (playerID == ClientConfig.PlayerID) {
                    // Tell the UI "playerID" stole someone's space.
                    SpaceStateChangeEvent?.Invoke(SpaceState.StolenGained, spaceID);
                }
            } else {
                Debug.Log(playerID + " claimed an empty space " + parkingSpace.SpaceID);
                if (playerID == ClientConfig.PlayerID) {
                    // Tell the UI "playerID" claimed an empty space.
                    SpaceStateChangeEvent?.Invoke(SpaceState.EmptyGained, spaceID);
                }
            }

            // If a player already claimed a space in this round, make sure they lose it.

            if (ParkingSpacesByPlayerID.ContainsKey(playerID)) {
                var previousSpace = ParkingSpacesByPlayerID[playerID];
                Debug.Log(playerID + " lost their previous space " + previousSpace.SpaceID);
                ParkingSpacesByPlayerID[playerID].SetEmpty();
            }

            ParkingSpacesByPlayerID[playerID] = parkingSpace;
            parkingSpace.SetOccupied(playerID);
        }

        /// <summary>
        /// Gets the transforms for all spaces.
        /// </summary>
        /// <returns>A list containing all space transforms</returns>
        public List<Transform> GetSpaceTransforms() {
            var transforms = new List<Transform>();
            foreach (var space in parkingSpacesBySpaceID) {
                transforms.Add(space.Value.transform);
            }

            return transforms;
        }
    }

    /// <summary>
    /// A ParkingSpaceManager but with methods specific to the client.
    /// </summary>
    public class ClientParkingSpaceManager : ParkingSpaceManager {
        public void SubscribeSpaceEnter(SpaceEnterDelegate f) {
            foreach (var p in parkingSpacesBySpaceID) {
                p.Value.SpaceEnterEvent += f;
            }
        }

        public void SubscribeSpaceExit(SpaceExitDelegate f) {
            foreach (var p in parkingSpacesBySpaceID) {
                p.Value.SpaceExitEvent += f;
            }
        }
    }

    /// <summary>
    /// A ParkingSpaceManager but with methods specific to the server.
    /// </summary>
    public class ServerParkingSpaceManager : ParkingSpaceManager {
        // Delegates
        public event SpaceClaimedDelegate SpaceClaimedEvent;

        
        /// <summary>
        /// Event handler for when a player enters a space.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="spaceID"></param>
        public void OnSpaceEnter(int playerID, ushort spaceID) {
            Debug.Log($"Player: {playerID} has entered space {spaceID}");
            // TODO: Should we inform the client we agree with their entry into the space?


            // Start a timer for a space if space does not belong to the player
            var parkingSpace = parkingSpacesBySpaceID[spaceID];

            if (!parkingSpace.Enabled) {
                return;
            }

            if (parkingSpace.OccupiedBy == playerID) {
                // Do nothing.
            } else if (parkingSpace.OccupiedBy == -1) {
                if (parkingSpace.Timer.Set) {
                    parkingSpace.Timer.Reset();
                    // Inform clients that the attempt to claim the space has failed.
                } else {
                    parkingSpace.Timer         =  new Timer(1);
                    parkingSpace.Timer.Elapsed += SpaceEnterTimerHandler(playerID, spaceID);
                    parkingSpace.Timer.Start();
                }
            } else {
                if (parkingSpace.Timer.Set) {
                    parkingSpace.Timer.Reset();
                    // Inform clients that the attempt to claim the space has failed.
                } else {
                    parkingSpace.Timer         =  new Timer(3);
                    parkingSpace.Timer.Elapsed += SpaceEnterTimerHandler(playerID, spaceID);
                    parkingSpace.Timer.Start();
                }
            }
        }

        /// <summary>
        /// Event handler for when a player leaves a space.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="spaceID"></param>
        public void OnSpaceExit(int playerID, ushort spaceID) {
            var parkingSpace = parkingSpacesBySpaceID[spaceID];

            // Cancel a timer for a space
            if (parkingSpace.Timer.Set) {
                parkingSpace.Timer.Reset();
                Debug.Log($"Player: {playerID} has left space before it is claimed, cancelling timer.");
                // Inform clients that the attempt to claim the space has failed. Is this one needed?
                // If the client knows they've left the space then they know its failed.
            }
        }
        
        private TimerOverDelegate SpaceEnterTimerHandler(int playerID, ushort spaceID) {
            return () => SpaceTimerOnElapsed(playerID, spaceID);
        }

        /// <summary>
        /// Event handler for when a timer on a space has elapsed.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="spaceID"></param>
        private void SpaceTimerOnElapsed(int playerID, ushort spaceID) {
            Debug.Log("Timer has elapsed");
            // Space timer has elapsed.
            OnSpaceClaimed(playerID, spaceID);
            SpaceClaimedEvent?.Invoke(playerID, spaceID);
        }

        /// <summary>
        /// Calculates the nearest spaces to a point.
        /// </summary>
        /// <param name="position">Location to get spaces close to.</param>
        /// <param name="amount">Number of spaces to return.</param>
        /// <returns>List of parking spaceIDs</returns>
        public List<ushort> GetNearestSpaces(Vector2 position, int amount) {
            var spaces = parkingSpacesBySpaceID.Values
                                               .OrderBy(space => {
                                                    var spacePos = space.transform.position;

                                                    return new Vector2(
                                                            spacePos.x - position.x, spacePos.z - position.y)
                                                       .magnitude;
                                                })
                                               .Take(amount)
                                               .Select(space => space.SpaceID)
                                               .ToList();

            return spaces;
        }
    }
}