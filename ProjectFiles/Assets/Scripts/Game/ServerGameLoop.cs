using Network;

namespace Game
{
    public class ServerGameLoop : IGameLoop
    {
        private Server server;
        private World world;
        private Spawner spawner;
        
        public bool Init(Spawner spawner, string[] args)
        {
            // Start server
            server = new Server();
            var success = server.Start();
            
            // Create world
            world = new World(spawner);
            
            
            return success;
        }

        public void Shutdown()
        {
            server.Shutdown();
//            world.Destroy();
        }

        public void Update()
        {
            server.HandleNetworkEvents();
        }

        public void FixedUpdate()
        {
        
        }

        public void LateUpdate()
        {
        
        }
    }
}
