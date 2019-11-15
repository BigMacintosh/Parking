using Network;
using UnityEngine;

namespace Game
{
    public class ClientGameLoop : IGameLoop
    {
        private Client client;
        private World world;

        public bool Init(string[] args)
        {
            // Create world
            world = new World();
            
            // Start server
            client = new Client(world);
            
            #if UNITY_EDITOR
                var success = client.Start();
            #else
                var success = client.Start("18.191.231.10");
            #endif
            
            return success;
        }

        public void Shutdown()
        {
            client.Shutdown();
//          Destroy the world here.
        }

        public void Update()
        {
            client.HandleNetworkEvents();
            world.Update();
        }

        public void FixedUpdate()
        {
            if (world.ClientID >= 0)
            {
                client.SendLocationUpdate();
            }
        }

        public void LateUpdate()
        {
            // Nothing required here yet.
        }
    }
}
