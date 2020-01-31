using Network;
using UnityEngine;

namespace Game
{
    public class ServerGameLoop : IGameLoop
    {
        private Server server;
        private World world;
        private RoundManager roundManager;

        public bool Init(string[] args)
        {
            // TODO: make the config path an argument so we can swap configs later?
            var config = ServerConfig.LoadConfigOrDefault("server-config.json");
            
            // Create world
            world = new World();

            // Start server
            server = new Server(world, config);
            var success = server.Start();
            
            roundManager = new RoundManager(world);
            
            
            // Subscribe to events.
            roundManager.GameStartEvent += server.OnStartGame;
            roundManager.RoundStartEvent += server.OnRoundStart;
            roundManager.PreRoundStartEvent += server.OnPreRoundStart;
            roundManager.RoundEndEvent += server.OnRoundEnd;
            roundManager.EliminatePlayersEvent += server.OnEliminatePlayers;

            return success;
        }

        public void Shutdown()
        {
            server.Shutdown();
//          Destroy the world here.
        }

        public void Update()
        {
            if(Input.GetKeyDown("a") && Input.GetKeyDown("b") && !roundManager.Started)
            {
                roundManager.StartGame();
            }
            server.HandleNetworkEvents();
            world.Update();
            
        }

        public void FixedUpdate()
        {
            // Trigger network send.
            server.SendLocationUpdates();
        }

        public void LateUpdate()
        {
            // Nothing required here yet.
        }
    }
}
