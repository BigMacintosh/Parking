using Network;

namespace Game
{
    public class ServerGameLoop : IGameLoop
    {
        private Server server;
        private World world;

        public bool Init(string[] args)
        {
            // TODO: make the config path an argument so we can swap configs later?
            var config = ServerConfig.LoadConfigOrDefault("server-config.json");
            
            // Create world
            world = new World();

            // Start server
            server = new Server(world, config);
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
