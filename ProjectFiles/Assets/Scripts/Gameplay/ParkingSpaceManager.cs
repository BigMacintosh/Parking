using System.Collections.Generic;
using System.Linq;
using Utils;
using Network;
using UnityEngine;

namespace Gameplay
{
    public class ParkingSpaceManager
    {
        protected Dictionary<ushort, ParkingSpace> parkingSpacesBySpaceID;
        protected Dictionary<int, ParkingSpace> parkingSpacesByPlayerID;
        public ParkingSpaceManager()
        {
            parkingSpacesBySpaceID = new Dictionary<ushort, ParkingSpace>();
            parkingSpacesByPlayerID = new Dictionary<int, ParkingSpace>();

            var parkingSpaceList = Object.FindObjectsOfType<ParkingSpace>();
            foreach (var parkingSpace in parkingSpaceList)
            {
                parkingSpacesBySpaceID.Add(parkingSpace.SpaceID, parkingSpace);
                Debug.Log($"Space Added, SpaceID: {parkingSpace.SpaceID}.");
            }
            DisableAllSpaces();
        }
        
        // OnRoundEnd
        public void DisableAllSpaces()
        {
            foreach (var space in parkingSpacesBySpaceID.Values)
            {
                space.Disable();
            }
            parkingSpacesByPlayerID.Clear();
        }
        
        // OnRoundStart
        public void EnableSpaces(List<ushort> spaces)
        {
            foreach (var space in spaces) 
            {
                parkingSpacesBySpaceID[space].Enable();
            }
        }
        
        public void OnSpaceClaimed(int playerID, ushort spaceID)
        {
            var parkingSpace = parkingSpacesBySpaceID[spaceID];
            if (parkingSpace.Occupied())
            {
                Debug.Log(playerID + " stole a space from " + parkingSpace.OccupiedBy);
            }
            else
            {
                Debug.Log(playerID + " claimed an empty space " + parkingSpace.SpaceID);
            }

            // If a player already claimed a space in this round, make sure they lose it.
            
            if (parkingSpacesByPlayerID.ContainsKey(playerID))
            {
                var previousSpace = parkingSpacesByPlayerID[playerID];
                Debug.Log(playerID + " lost their previous space " + previousSpace.SpaceID);
                parkingSpacesByPlayerID[playerID].SetEmpty();
            }

            parkingSpacesByPlayerID[playerID] = parkingSpace;
            parkingSpace.SetOccupied(playerID);
        }

        public List<Transform> GetSpaceTransforms()
        {
            List<Transform> transforms = new List<Transform>();
            foreach (var space in parkingSpacesBySpaceID)
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
            foreach (var p in parkingSpacesBySpaceID)
            {
                p.Value.SpaceEnterEvent += f;
            }
        }

        public void SubscribeSpaceExit(SpaceExitDelegate f)
        {
            foreach (var p in parkingSpacesBySpaceID)
            {
                p.Value.SpaceExitEvent += f;
            }
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
            var parkingSpace = parkingSpacesBySpaceID[spaceID];
            
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
            var parkingSpace = parkingSpacesBySpaceID[spaceID];

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
            OnSpaceClaimed(playerID, spaceID);
            SpaceClaimedEvent?.Invoke(playerID, spaceID);
        }

        public List<ushort> GetNearestSpaces(Vector2 position, int amount)
        {
            List<ushort> spaces = parkingSpacesBySpaceID.Values.OrderBy(space => {
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