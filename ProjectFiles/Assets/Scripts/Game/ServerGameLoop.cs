using Gameplay;
using Network;
using UI;
using UnityEngine;

namespace Game
{
    public class ServerGameLoop : IGameLoop
    {
        private Server server;
        private World world;
        private RoundManager roundManager;
        private ServerParkingSpaceManager parkingSpaceManager;
        
#if UNITY_EDITOR
        private UIController uiController;
#endif
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
            
#if UNITY_EDITOR
            uiController = Object.Instantiate(Resources.Load<GameObject>("UICanvas"), Vector3.zero, Quaternion.identity).GetComponent<UIController>();
            uiController.IsServerMode = true;
            uiController.SubscribeTriggerGameStartEvent(roundManager.StartGame);
            roundManager.GameStartEvent += uiController.OnGameStart;
#endif
            
            // Subscribe to network events.
            // Client -> Server
            server.SpaceEnterEvent += parkingSpaceManager.OnSpaceEnter;
            server.SpaceExitEvent += parkingSpaceManager.OnSpaceExit;
            server.TriggerGameStartEvent += roundManager.StartGame;
            
            // Server -> Client
            roundManager.GameStartEvent += server.OnStartGame;
            roundManager.RoundStartEvent += server.OnRoundStart;
            roundManager.PreRoundStartEvent += server.OnPreRoundStart;
            roundManager.PreRoundStartEvent += (number, length, roundLength, players, active) =>
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
