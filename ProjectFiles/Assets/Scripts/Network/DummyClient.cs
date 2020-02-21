using Game;
using UnityEngine;

namespace Network
{
    class DummyClient : IClient
    {
        private World world;
        private int playerID;
            
            
        public DummyClient(World world)
        {
            this.world = world;
        }
        public bool Start(string ip, ushort port)
        {
            world.ClientID = 0;
            world.SpawnPlayer(world.ClientID);
            world.SetPlayerControllable(world.ClientID);
            return true;
        }

        public void Shutdown()
        {
                
        }

        public void SendLocationUpdate()
        {
                
        }

        public void HandleNetworkEvents()
        {
                
        }
        public event GameStartDelegate GameStartEvent;
        public event PreRoundStartDelegate PreRoundStartEvent;
        public event RoundStartDelegate RoundStartEvent;
        public event RoundEndDelegate RoundEndEvent;
        public event SpaceClaimedDelegate SpaceClaimedEvent;
        public event EliminatePlayersDelegate EliminatePlayersEvent;
        public event GameEndDelegate GameEndEvent;

        public void OnSpaceEnter(int playerID, ushort spaceID)
        {
            Debug.Log($"Someone entered the space #{spaceID}");   
        }

        public void OnSpaceExit(int playerID, ushort spaceID)
        {
            Debug.Log($"Someone exited the space #{spaceID}");
        }
    }
}