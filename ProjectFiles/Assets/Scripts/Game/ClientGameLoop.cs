using Network;
using UnityEngine;
using Gameplay;

namespace Game
{
    public class ClientGameLoop : IGameLoop
    {
        private IClient client;
        private World world;
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
            world = new World();
            parkingSpaceManager = new ClientParkingSpaceManager();

            // Create HUD
            Object.Instantiate(Resources.Load<GameObject>("Canvas"), Vector3.zero, Quaternion.identity);
            Object.Instantiate(Resources.Load<GameObject>("Minimap Canvas"), Vector3.zero, Quaternion.identity);
            
            
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
            parkingSpaceManager.SubscribeSpaceEnter(client.OnSpaceEnter);
            parkingSpaceManager.SubscribeSpaceExit(client.OnSpaceExit);
            
            // Server -> Client
            client.PreRoundStartEvent += (number, length, roundLength, players, active) =>
                Debug.Log($"PreRoundStart event received rN:{number} preLength:{length} roundLength:{roundLength} nP:{players}");
            client.RoundStartEvent += number => Debug.Log($"Round start event received rN:{number}");
            client.RoundEndEvent += number => Debug.Log($"Round end event received rN:{number}");
            client.SpaceClaimedEvent += parkingSpaceManager.OnSpaceClaimed;
            
            // Start the client connection
#if UNITY_EDITOR
            var success = client.Start();
#else
            var success = client.Start("35.177.253.83");
#endif
            
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
