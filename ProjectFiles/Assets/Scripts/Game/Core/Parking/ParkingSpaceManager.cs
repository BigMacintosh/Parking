using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Network;
using UI;
using UnityEngine;
using Utils;
using Transform = UnityEngine.Transform;

namespace Game.Core.Parking {
    /// <summary>
    /// Generic ParkingSpaceManager
    /// Used to control all the parking spaces in the game.
    /// </summary>
    public abstract class ParkingSpaceManager {
        // Public fields
        public Dictionary<int, ParkingSpaceController> ParkingSpacesByPlayerID { get; private set; }

        // Protected fields
        protected Dictionary<ushort, ParkingSpaceController> parkingSpacesBySpaceID;

        /// <summary>
        ///  Constructor for ParkingSpaceManager
        /// </summary>
        protected ParkingSpaceManager() {
            parkingSpacesBySpaceID  = new Dictionary<ushort, ParkingSpaceController>();
            ParkingSpacesByPlayerID = new Dictionary<int, ParkingSpaceController>();

            var parkingSpaceList = Object.FindObjectsOfType<ParkingSpace>();
            foreach (var parkingSpace in parkingSpaceList) {
                parkingSpacesBySpaceID.Add(parkingSpace.Controller.SpaceID, parkingSpace.Controller);
            }

            DisableAllSpaces();
        }

        public void TEST_ONLY_AddParkingSpace(ParkingSpaceController p) {
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
        public List<ObjectTransform> GetSpaceTransforms() {
            var transforms = new List<ObjectTransform>();
            foreach (var space in parkingSpacesBySpaceID) {
                transforms.Add(space.Value.Transform);
            }

            return transforms;
        }

        /// <summary>
        /// Calculates the nearest spaces to a point.
        /// </summary>
        /// <param name="position">Location to get spaces close to.</param>
        /// <param name="amount">Number of spaces to return.</param>
        /// <param name="onlyEnabled">Look only for spaces that are enabled.</param>
        /// <returns>List of parking spaceIDs</returns>
        public List<ushort> GetNearestSpaces(Vector2 position, int amount, bool onlyEnabled = false) {
            var spaces = parkingSpacesBySpaceID.Values
                                               .OrderBy(space => {
                                                    var spacePos = space.Transform.Position;
                                                    return new Vector2(
                                                            spacePos.x - position.x, spacePos.z - position.y)
                                                       .magnitude;
                                                })
                                               .ToList();

            if (onlyEnabled) {
                spaces = spaces.FindAll(s => s.Enabled);
            }

            return spaces
                  .Take(amount)
                  .Select(space => space.SpaceID)
                  .ToList();
        }
    }
}