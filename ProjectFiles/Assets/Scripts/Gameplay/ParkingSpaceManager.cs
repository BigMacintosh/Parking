using System.Collections.Generic;
using System.Linq;
using Utils;
using Network;
using UnityEngine;

namespace Gameplay
{
    public class ParkingSpaceManager
    {
        protected Dictionary<ushort, ParkingSpace> parkingSpaces;

        public ParkingSpaceManager()
        {
            parkingSpaces = new Dictionary<ushort, ParkingSpace>();
            var parkingSpaceList = Object.FindObjectsOfType<ParkingSpace>();
            foreach (var parkingSpace in parkingSpaceList)
            {
                parkingSpaces.Add(parkingSpace.SpaceID, parkingSpace);
                Debug.Log($"Space Added, SpaceID: {parkingSpace.SpaceID}.");
            }
        }
        
        private void DisableAllSpaces()
        {
            foreach (var space in parkingSpaces.Values)
            {
                space.Disable();
            }
        }
        // OnPreRoundStart: Disable all spaces and only enable the new spaces.
        public void EnableSpaces(List<ushort> spaces)
        {
            DisableAllSpaces();
            foreach (var space in spaces) 
            {
                parkingSpaces[space].Enable();
            }
        }

        public List<Transform> GetSpaceTransforms()
        {
            List<Transform> transforms = new List<Transform>();
            foreach (var space in parkingSpaces)
            {
                transforms.Add(space.Value.transform);
            }
            return transforms;
        }
    }

    public class ClientParkingSpaceManager : ParkingSpaceManager
    {
        public void SubscribeSpaceEnter(SpaceEnterDelegate f)
        {
            foreach (var p in parkingSpaces)
            {
                p.Value.SpaceEnterEvent += f;
            }
        }

        public void SubscribeSpaceExit(SpaceExitDelegate f)
        {
            foreach (var p in parkingSpaces)
            {
                p.Value.SpaceExitEvent += f;
            }
        }

        public void OnSpaceClaimed(int playerID, ushort spaceID)
        {
            var parkingSpace = parkingSpaces[spaceID];
            if (parkingSpace.Occupied())
            {
                Debug.Log(playerID + " Stole a space from " + parkingSpace.OccupiedBy);
            }
            else
            {
                Debug.Log(playerID + " CLAIMED AN EMPTY SPACE" + spaceID);
            }

            parkingSpace.SetOccupied(playerID);
        }
    }

    public class ServerParkingSpaceManager : ParkingSpaceManager
    {
        public event SpaceClaimedDelegate SpaceClaimedEvent;

        public void OnSpaceEnter(int playerID, ushort spaceID)
        {
            Debug.Log($"Player: {playerID} has entered space {spaceID}"); 
            // TODO: Should we inform the client we agree with their entry into the space?


            // Start a timer for a space if space does not belong to the player
            var parkingSpace = parkingSpaces[spaceID];
            
            if (!parkingSpace.Enabled)
            {
                return;
            }
            
            if (parkingSpace.OccupiedBy == playerID)
            {
                // Do nothing.
            }
            else if (parkingSpace.OccupiedBy == -1)
            {
                if (parkingSpace.Timer.Set)
                {
                    parkingSpace.Timer.Reset();
                    // Inform clients that the attempt to claim the space has failed.
                }
                else
                {
                    parkingSpace.Timer = new Timer(1);
                    parkingSpace.Timer.Elapsed += SpaceEnterTimerHandler(playerID, spaceID);
                    parkingSpace.Timer.Start();
                }
            }
            else
            {
                if (parkingSpace.Timer.Set)
                {
                    parkingSpace.Timer.Reset();
                    // Inform clients that the attempt to claim the space has failed.
                }
                else
                {
                    parkingSpace.Timer = new Timer(3);
                    parkingSpace.Timer.Elapsed += SpaceEnterTimerHandler(playerID, spaceID);
                    parkingSpace.Timer.Start();
                }
            }
        }

        public void OnSpaceExit(int playerID, ushort spaceID)
        {
            var parkingSpace = parkingSpaces[spaceID];

            // Cancel a timer for a space
            if (parkingSpace.Timer.Set)
            {
                parkingSpace.Timer.Reset();
                Debug.Log($"Player: {playerID} has left space before it is claimed, cancelling timer.");
                    // Inform clients that the attempt to claim the space has failed. Is this one needed?
                // If the client knows they've left the space then they know its failed.
            }
        }

        private TimerOverDelegate SpaceEnterTimerHandler(int playerID, ushort spaceID)
        {
            return () => SpaceTimerOnElapsed(playerID, spaceID);
        }

        private void SpaceTimerOnElapsed(int playerID, ushort spaceID)
        {
            Debug.Log("Timer has elapsed");
            // Space timer has elapsed.
            parkingSpaces[spaceID].OccupiedBy = playerID;
            SpaceClaimedEvent?.Invoke(playerID, spaceID);
        }

        public List<ushort> GetNearestSpaces(Vector2 position, int amount)
        {
            List<ushort> spaces = parkingSpaces.Values.OrderBy(space => {
                    var spacePos = space.transform.position;

                    return new Vector2(spacePos.x - position.x, spacePos.z - position.y).magnitude;
                })
                .Take(amount)
                .Select(space => space.SpaceID)
                .ToList();

            return spaces;
        }
    }
}