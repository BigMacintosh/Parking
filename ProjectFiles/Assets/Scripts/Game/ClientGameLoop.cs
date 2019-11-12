using Network;

namespace Game
{
    public class ClientGameLoop : IGameLoop
    {
        private Client client;
        private World world;

        public bool Init(Spawner spawner, string[] args)
        {
            // Create world
            world = new World(spawner);
            
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
            world.Update();
            client.HandleNetworkEvents();
        }

        public void FixedUpdate()
        {
            // Nothing required here yet.
        }

        public void LateUpdate()
        {
            // Nothing required here yet.
        }
    }
}
