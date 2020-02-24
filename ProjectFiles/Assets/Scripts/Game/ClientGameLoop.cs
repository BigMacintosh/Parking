using System.Collections.Generic;
using Network;
using UnityEngine;
using Gameplay;
using UI;
using Object = UnityEngine.Object;

namespace Game
{
    public class ClientGameLoop : IGameLoop
    {
        private IClient client;
        private World world;
        private UIController uiController;
        private bool isStandalone;
        private ClientParkingSpaceManager parkingSpaceManager;
        
        public bool Init(string[] args)
        {
            
            // Determine if in standalone mode
            if (args.Length > 0)
            {
                isStandalone = args[0].Equals("standalone");
            }
            else
            {
                isStandalone = false;
            }

            // Create gameplay components
            
            //if (ClientConfig.GameMode == GameMode.PlayerMode)
            //{
                parkingSpaceManager = new ClientParkingSpaceManager();
            //}
            world = new World(parkingSpaceManager);

            // Create UI Controller
            if (ClientConfig.GameMode == GameMode.PlayerMode)
            {
                Object.Instantiate(Resources.Load<GameObject>("Minimap Canvas"), Vector3.zero, Quaternion.identity);
            }
            uiController = Object.Instantiate(Resources.Load<GameObject>("UICanvas"), Vector3.zero, Quaternion.identity).GetComponent<UIController>();
            

            // Initialise the client
            if (isStandalone)
            {
                client = Client.getDummyClient(world);
            }
            else
            {
                client = new Client(world);
            }


            // Subscribe to network events.
            // Client -> Server
            if (ClientConfig.GameMode == GameMode.PlayerMode)
            {
                parkingSpaceManager.SubscribeSpaceEnter(client.OnSpaceEnter);
                parkingSpaceManager.SubscribeSpaceExit(client.OnSpaceExit);
            }

            // Server -> Client
            client.PreRoundStartEvent += (number, length, roundLength, players) =>
                Debug.Log($"PreRoundStart event received rN:{number} preLength:{length} roundLength:{roundLength} nP:{players}");

            
            client.RoundStartEvent += (number, active) =>
            {
                Debug.Log($"Round start event received rN:{number}");
                parkingSpaceManager.EnableSpaces(active);
            };
            
            client.RoundEndEvent += number =>
            {
                Debug.Log($"Round end event received rN:{number}");
                parkingSpaceManager.DisableAllSpaces();
            };
            
            client.SpaceClaimedEvent += parkingSpaceManager.OnSpaceClaimed;
            
            // Start the client connection
            var success = client.Start();

            uiController.getHUD().NumberOfPlayers = world.Players.Count;
            uiController.getHUD().NetworkIP = client.getServerIP();
            uiController.getHUD().exitButton.onClick.AddListener(Shutdown);
            client.GameStartEvent += uiController.OnGameStart;
            client.PreRoundStartEvent += uiController.OnPreRoundStart;
            client.RoundStartEvent += uiController.OnRoundStart;
            client.RoundEndEvent += uiController.OnRoundEnd;
            client.PlayerCountChangeEvent += uiController.OnPlayerCountChange;

            if (ClientConfig.GameMode == GameMode.AdminMode)
            {
                uiController.SubscribeTriggerGameStartEvent(client.OnTriggerGameStart);
            }

            return success;
        }

        public void Shutdown()
        {
            client.Shutdown();
//          Destroy the world here.
        }

        public void Update()
        {
            client.HandleNetworkEvents();
            world.Update();
        }

        public void FixedUpdate()
        {
            if (world.ClientID >= 0)
            {
                client.SendLocationUpdate();
            }
        }

        public void LateUpdate()
        {
            // Nothing required here yet.
        }
    }
}
