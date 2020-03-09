using Game.Core.Parking;
using Game.Core.Rounds;
using Game.Entity;
using Network;
using UI;
using UnityEngine;

namespace Game.Main {
    /// <summary>
    /// The game loop for a server.
    /// Initialises all the components needed on the server and ensures that they are updated correctly.
    /// </summary>
    public class ServerGameLoop : IGameLoop {
        // Fields
        private ServerParkingSpaceManager parkingSpaceManager;
        private RoundManager              roundManager;
        private Server                    server;
        private ServerWorld               world;

    #if UNITY_EDITOR
        private UIController uiController;
    #endif

        public bool Init(string[] args) {
            // Load default server config
            var config = ServerConfig.LoadConfigOrDefault("server-config.json");

            // Initialise Gameplay components
            parkingSpaceManager = new ServerParkingSpaceManager();
            world               = new ServerWorld(parkingSpaceManager);
            roundManager        = new RoundManager(world, parkingSpaceManager);

            // Initialise network
            server = new Server(world, config);

            // Subscribe to network events.
            // Client -> Server
            server.SpaceEnterEvent       += parkingSpaceManager.OnSpaceEnter;
            server.SpaceExitEvent        += parkingSpaceManager.OnSpaceExit;
            server.TriggerGameStartEvent += roundManager.StartGame;

            // Server -> Client
            roundManager.GameStartEvent        += server.OnStartGame;
            roundManager.RoundStartEvent       += server.OnRoundStart;
            roundManager.PreRoundStartEvent    += server.OnPreRoundStart;
            roundManager.RoundEndEvent         += server.OnRoundEnd;
            roundManager.EliminatePlayersEvent += server.OnEliminatePlayers;
            roundManager.GameEndEvent          += server.OnGameEnd;

            roundManager.RoundStartEvent       += parkingSpaceManager.OnRoundStart;
            roundManager.RoundEndEvent         += parkingSpaceManager.OnRoundEnd;
            roundManager.EliminatePlayersEvent += world.OnEliminatePlayers;

            parkingSpaceManager.SpaceClaimedEvent += server.OnSpaceClaimed;

            // Allow a server in the unity editor to act as an admin client
        #if UNITY_EDITOR
            uiController = Object.Instantiate(Resources.Load<GameObject>("UICanvas"), Vector3.zero, Quaternion.identity)
                                 .GetComponent<UIController>();
            uiController.IsServerMode = true;
            uiController.SubscribeTriggerGameStartEvent(roundManager.StartGame);
            roundManager.GameStartEvent += uiController.OnGameStart;
        #endif

            // Start server
            var success = server.Start();

            return success;
        }

        public void Shutdown() {
            server.Shutdown();
        }

        public void Update() {
            server.HandleNetworkEvents();
            // world.Update();
            roundManager.Update();
            parkingSpaceManager.Update();
        }

        public void FixedUpdate() {
            // Trigger network send.
            server.SendEvents();
        }

        public void LateUpdate() {
            // Nothing required here yet.
        }

        /// <summary>
        /// Resets the game when it has finished.
        /// </summary>
        public void Reset() {
            world.Reset();
            // TODO: Add other game components here.
        }
    }
}