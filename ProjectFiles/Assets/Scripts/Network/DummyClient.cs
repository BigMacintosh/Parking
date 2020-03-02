using Game;
using Game.Entity;
using UnityEngine;

namespace Network {
    internal class DummyClient : IClient {
        // Delegates
        public event GameStartDelegate         GameStartEvent;
        public event PreRoundStartDelegate     PreRoundStartEvent;
        public event RoundStartDelegate        RoundStartEvent;
        public event RoundEndDelegate          RoundEndEvent;
        public event SpaceClaimedDelegate      SpaceClaimedEvent;
        public event EliminatePlayersDelegate  EliminatePlayersEvent;
        public event PlayerCountChangeDelegate PlayerCountChangeEvent;
        public event GameEndDelegate           GameEndEvent;

        private readonly ClientWorld world;


        public DummyClient(ClientWorld world) {
            this.world = world;
        }

        public bool Start(ushort port = 25565) {
            world.CreatePlayer(0, new PlayerOptions {
                CarType = CarType.Hatchback,
            }, true);
            world.SpawnPlayers();
            return true;
        }

        public void Shutdown() { }

        public void SendLocationUpdate() { }

        public void HandleNetworkEvents() { }

        public string GetServerIP() {
            return "Standalone";
        }

        public void OnSpaceEnter(int playerID, ushort spaceID) {
            Debug.Log($"Someone entered the space #{spaceID}");
        }

        public void OnSpaceExit(int playerID, ushort spaceID) {
            Debug.Log($"Someone exited the space #{spaceID}");
        }

        public void OnTriggerGameStart() { }
    }
}