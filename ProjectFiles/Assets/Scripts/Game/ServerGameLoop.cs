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

            this.spawner = spawner;
            
            return success;
        }

        public void Shutdown()
        {
            server.Shutdown();
//          Destroy the world here.
        }

        public void Update()
        {
            server.HandleNetworkEvents();
            world.Update();
        }

        public void FixedUpdate()
        {
            // Trigger network send.
        }

        public void LateUpdate()
        {
            // Nothing required here yet.
        }
    }
}
