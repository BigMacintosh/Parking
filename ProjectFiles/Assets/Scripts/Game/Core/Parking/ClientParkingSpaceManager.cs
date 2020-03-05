using Network;
using UI;
using UnityEngine;

namespace Game.Core.Parking {
    /// <summary>
    /// A ParkingSpaceManager but with methods specific to the client.
    /// </summary>
    public class ClientParkingSpaceManager : ParkingSpaceManager {
        public event SpaceStateChangeDelegate SpaceStateChangeEvent;

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

        /// <summary>
        /// Event handler for when a space is claimed. Performs a few client-specific tasks.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="spaceID"></param>
        public override void OnSpaceClaimed(int playerID, ushort spaceID) {
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
    }
}