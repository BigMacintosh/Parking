using Game.Core.Parking;
using Game.Entity;
using Network;
using UI;
using UnityEngine;

namespace Game.Main {
    public class ClientGameLoop : IGameLoop {
        // Private Fields
        private IClient                   client;
        private bool                      isStandalone;
        private ClientParkingSpaceManager parkingSpaceManager;
        private UIController              uiController;
        private ClientWorld               world;

        public bool Init(string[] args) {
            // Determine if in standalone mode
            isStandalone = args.Length > 0 && args[0].Equals("standalone");

            // Create gameplay components
            parkingSpaceManager = new ClientParkingSpaceManager();
            world               = new ClientWorld();

            // Create UI Controller
            if (ClientConfig.GameMode == GameMode.PlayerMode) {
                Object.Instantiate(Resources.Load<GameObject>("Minimap Canvas"), Vector3.zero, Quaternion.identity);
            }

            uiController = Object.Instantiate(Resources.Load<GameObject>("UICanvas"), Vector3.zero, Quaternion.identity)
                                 .GetComponent<UIController>();

            // Initialise the client
            client = isStandalone ? Client.GetDummyClient(world) : new Client(world);


            // Subscribe to network events.
            // Client -> Server
            parkingSpaceManager.SubscribeSpaceEnter(client.OnSpaceEnter);
            parkingSpaceManager.SubscribeSpaceExit(client.OnSpaceExit);
            parkingSpaceManager.SpaceStateChangeEvent += uiController.OnSpaceStateChange;

            if (ClientConfig.GameMode == GameMode.AdminMode) {
                uiController.SubscribeTriggerGameStartEvent(client.OnTriggerGameStart);
            }

            // Server -> Client
            client.PreRoundStartEvent += (number, length, roundLength, players) =>
                Debug.Log(
                    $"PreRoundStart event received rN:{number} preLength:{length} roundLength:{roundLength} nP:{players}");

            client.RoundStartEvent += (number, active) => {
                Debug.Log($"Round start event received rN:{number}");
                parkingSpaceManager.EnableSpaces(active);
            };

            client.RoundEndEvent += number => {
                Debug.Log($"Round end event received rN:{number}");
                parkingSpaceManager.DisableAllSpaces();
            };

            client.GameStartEvent         += uiController.OnGameStart;
            client.PreRoundStartEvent     += uiController.OnPreRoundStart;
            client.RoundStartEvent        += uiController.OnRoundStart;
            client.RoundEndEvent          += uiController.OnRoundEnd;
            client.PlayerCountChangeEvent += uiController.OnPlayerCountChange;
            client.GameEndEvent           += uiController.OnGameEnd;
            client.EliminatePlayersEvent  += uiController.OnEliminatePlayers;
            client.SpaceClaimedEvent      += parkingSpaceManager.OnSpaceClaimed;

            // Game -> UI
            uiController.OnPlayerCountChange(world.GetNumPlayers());
            uiController.Hud.NetworkIP = client.GetServerIP();
            uiController.Hud.exitButton.onClick.AddListener(Shutdown);

            // Start the client connection
            var success = client.Start();

            return success;
        }

        public void Shutdown() {
            client.Shutdown();
//          Destroy the world here.
        }

        public void Update() {
            client.HandleNetworkEvents();
            // world.Update();
        }

        public void FixedUpdate() {
            client.SendLocationUpdate();
        }

        public void LateUpdate() {
            // Nothing required here yet.
        }
    }
}