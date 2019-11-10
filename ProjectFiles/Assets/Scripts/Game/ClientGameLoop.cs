using Network;

namespace Game
{
    public class ClientGameLoop : IGameLoop
    {
        private Client client;
        private World world;

        public bool Init(Spawner spawner, string[] args)
        {
            // Start server
            client = new Client();
            
            #if UNITY_EDITOR
                var success = client.Start();
            #else
                var success = client.Start("18.191.231.10");
            #endif
            
            // Create world
            world = new World(spawner);
            
            
            return success;
        }

        public void Shutdown()
        {
            client.Shutdown();
//            world.Destroy();
        }

        public void Update()
        {
            client.HandleNetworkEvents();
        }

        public void FixedUpdate()
        {
        
        }

        public void LateUpdate()
        {
        
        }
    }
}
