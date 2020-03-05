using System.Collections.Generic;
using System.Linq;
using Network;
using UI;
using UnityEngine;
using Utils;

namespace Game.Core.Parking {
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
            } else if (!parkingSpace.Occupied()) {
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

        public override void OnSpaceClaimed(int playerID, ushort spaceID) {
            var parkingSpace = parkingSpacesBySpaceID[spaceID];
            if (parkingSpace.Occupied()) {
                Debug.Log(playerID + " stole a space from " + parkingSpace.OccupiedBy);
            } else {
                Debug.Log(playerID + " claimed an empty space " + parkingSpace.SpaceID);
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