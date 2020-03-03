using System.Collections;
using System.Collections.Generic;
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
            var playerID = 0;
            world.CreatePlayer(playerID, new PlayerOptions {
                CarType = CarType.Hatchback,
            }, true);
            var spawnPos = new PlayerPosition {
                Pos = new Vector3 {
                    x = -6.82f,
                    y = 2.19f,
                    z = -0.7f,
                },
                Rot = new Quaternion(),
            };
            var dict = new Dictionary<int, PlayerPosition> {{playerID, spawnPos}};
            world.SpawnPlayers(dict);
            return true;
        }

        public void Shutdown() { }

        public void SendEvents() { }

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