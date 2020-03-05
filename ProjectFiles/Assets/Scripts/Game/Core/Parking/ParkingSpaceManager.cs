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
    public abstract class ParkingSpaceManager {
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

        public void TEST_ONLY_AddParkingSpace(ParkingSpace p) {
            parkingSpacesBySpaceID.Add(p.SpaceID, p);
            p.Disable();
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
        private void EnableSpaces(List<ushort> spaces) {
            foreach (var space in spaces) {
                parkingSpacesBySpaceID[space].Enable();
            }
        }

        public void OnRoundStart(ushort roundNumber, List<ushort> spacesActive) {
            EnableSpaces(spacesActive);
        }

        public void OnPreRoundStart(ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort nPlayers) {
            DisableAllSpaces();
        }

        /// <summary>
        /// Event handler for when a space is claimed. Must be implemented by both client and server.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="spaceID"></param>
        public abstract void OnSpaceClaimed(int playerID, ushort spaceID);

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
}