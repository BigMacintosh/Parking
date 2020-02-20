using System.Collections.Generic;
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
                Debug.Log($"Space Added, SpaceID: {parkingSpace.SpaceID}");
                parkingSpaces.Add(parkingSpace.SpaceID, parkingSpace);
            }
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
    }

    public class ServerParkingSpaceManager : ParkingSpaceManager
    {
        public void OnSpaceEnter(int playerID, ushort spaceID)
        {
            Debug.Log($"Player: {playerID} has entered space {spaceID}");
            // Start a timer for a space if space does not belong to the player
            var parkingSpace = parkingSpaces[spaceID];
            if (parkingSpace.OccupiedBy == playerID)
            {
                // Do nothing.
            } 
            else if (parkingSpace.OccupiedBy == -1)
            {
                if (parkingSpace.Timer.Set)
                {
                    parkingSpace.Timer.Reset();
                }
                else
                {
                    parkingSpace.Timer = new Timer(1);
                    parkingSpace.Timer.Elapsed += SpaceTimerOnElapsed;
                    parkingSpace.Timer.Start();
                }
            }
            else
            {
                if (parkingSpace.Timer.Set)
                {
                    parkingSpace.Timer.Reset();
                }
                else
                {
                    parkingSpace.Timer = new Timer(3);
                    parkingSpace.Timer.Elapsed += SpaceTimerOnElapsed;
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
            }
        }
        
        private void SpaceTimerOnElapsed()
        {
            Debug.Log("Timer has elapsed");
            // Space timer has elapsed.
            // TODO: Maybe find a way to set the delegate type of a timer??
            
        }
    }
}
