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
            this.spawner = spawner;
            
            // Create world
            world = new World(spawner);

            // Start server
            server = new Server(world);
            var success = server.Start();
            
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
