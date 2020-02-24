using Gameplay;
using Network;
using UnityEngine;

namespace Game
{
    public class ServerGameLoop : IGameLoop
    {
        private Server server;
        private World world;
        private RoundManager roundManager;
        private ServerParkingSpaceManager parkingSpaceManager;

        public bool Init(string[] args)
        {
            // Load default server config
            var config = ServerConfig.LoadConfigOrDefault("server-config.json");
            
            // Initialise Gameplay components
            parkingSpaceManager = new ServerParkingSpaceManager();
            world = new World(parkingSpaceManager);
            roundManager = new RoundManager(world, parkingSpaceManager);

            // Initialise network
            server = new Server(world, config);

            // Subscribe to network events.
            // Client -> Server
            server.SpaceEnterEvent += parkingSpaceManager.OnSpaceEnter;
            server.SpaceExitEvent += parkingSpaceManager.OnSpaceExit;
            
            // Server -> Client
            roundManager.GameStartEvent += server.OnStartGame;
            roundManager.RoundStartEvent += server.OnRoundStart;
            roundManager.PreRoundStartEvent += server.OnPreRoundStart;
            roundManager.RoundStartEvent += (number, active) =>
                parkingSpaceManager.EnableSpaces(active);
            roundManager.RoundEndEvent += server.OnRoundEnd;
            roundManager.EliminatePlayersEvent += server.OnEliminatePlayers;
            roundManager.GameEndEvent += server.OnGameEnd;

            parkingSpaceManager.SpaceClaimedEvent += server.OnSpaceClaimed;

            // Start server
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
            if(Input.GetKeyDown("a") && Input.GetKeyDown("b") && !roundManager.GameInProgress)
            {
                roundManager.StartGame();
            }
            server.HandleNetworkEvents();
            world.Update();
            roundManager.Update();
            
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
