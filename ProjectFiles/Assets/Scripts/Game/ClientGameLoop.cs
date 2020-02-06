using Network;
using UnityEngine;
using UI;

namespace Game
{
    public class ClientGameLoop : IGameLoop
    {
        private IClient client;
        private World world;
        private UIController uiController;
        private bool isStandalone;
        
        public bool Init(string[] args)
        {
            if (args.Length > 0)
            {
                isStandalone = args[0].Equals("standalone");
            }
            else
            {
                isStandalone = false;
            }

            // Create world
            world = new World();

            // Create HUD
            Object.Instantiate(Resources.Load<GameObject>("Minimap Canvas"), Vector3.zero, Quaternion.identity);
            uiController = Object.Instantiate(Resources.Load<GameObject>("UICanvas"), Vector3.zero, Quaternion.identity).GetComponent<UIController>();
            
            // Start client connection
            if (isStandalone)
            {
                client = Client.getDummyClient(world);
            }
            else
            {
                client = new Client(world);
            }
            
#if UNITY_EDITOR
            var success = client.Start();
#else
            var success = client.Start("35.177.253.83");
#endif

            // Create HUD class
            
            // Subscribe HUD to client events.
            client.PreRoundStartEvent += (number, length, roundLength, players, active) =>
                Debug.Log($"PreRoundStart event received rN:{number} preLength:{length} roundLength:{roundLength} nP:{players}");
            client.RoundStartEvent += number => Debug.Log($"Round start event received rN:{number}");
            client.RoundEndEvent += number => Debug.Log($"Round end event received rN:{number}");
            
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
