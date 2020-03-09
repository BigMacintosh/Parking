using Game.Core.Parking;
using Game.Core.Rounds;
using Game.Entity;
using Game.Main;
using Network;
using NSubstitute;
using NUnit.Framework;
using UI;
using UnityEngine;
using Utils;
using Transform = UnityEngine.Transform;

namespace Tests {
    public class TestGameLoop : IGameLoop {
        // Fields
        private ServerParkingSpaceManager parkingSpaceManager;
        private RoundManager              roundManager;
        private ServerWorld               world;

        private Timer timer;
        public  bool  TestFinished;

        public bool Init(string[] args) {
            timer = new Timer(0);

            TestFinished = false;

            // Initialise Gameplay components
            parkingSpaceManager = new ServerParkingSpaceManager();

            for (int i = 0; i < 10; i++) {
                parkingSpaceManager.TEST_ONLY_AddParkingSpace(NewMockParkingSpaceController(i));
            }

            world        = new ServerWorld(parkingSpaceManager);
            roundManager = new RoundManager(world, parkingSpaceManager);

            world.CreatePlayer(0, new PlayerOptions());
            world.CreatePlayer(1, new PlayerOptions());
            world.CreatePlayer(2, new PlayerOptions());

            roundManager.GameStartEvent += (length, players) => world.SpawnPlayers();
            roundManager.GameEndEvent += winners => {
                Assert.True(winners.Count == 2);
                Assert.True(winners.Contains(1));
                Assert.True(winners.Contains(2));

                winners.ForEach(world.DestroyPlayer);
                TestFinished = true;
            };


            roundManager.RoundStartEvent    += parkingSpaceManager.OnRoundStart;
            roundManager.PreRoundStartEvent += parkingSpaceManager.OnPreRoundStart;

            roundManager.RoundStartEvent += (number, active) => {
                var t = new Timer(1);
                t.Elapsed += () => parkingSpaceManager.OnSpaceEnter(0, active[0]);
                t.Start();
            };

            roundManager.StartGame();
            return true;
        }

        private static ParkingSpaceController NewMockParkingSpaceController(int n) {
            var transformController = Substitute.For<ISpaceTransformController>();
            transformController.GetTransform().Returns(new ObjectTransform {
                Position = new Vector3(n, n),
                Rotation = new Quaternion(n, n, n, n),
            });

            var colourController = Substitute.For<ISpaceColourController>();
            var parkingSpace = Substitute.For<ParkingSpaceController>();
            parkingSpace.TransformController = transformController;
            parkingSpace.ColourController = colourController;
            return parkingSpace;
        }

        public void Shutdown() { }

        public void Update() {
            roundManager.Update();
            parkingSpaceManager.Update();
            timer.Update();
        }

        public void FixedUpdate() { }

        public void LateUpdate() { }
    }
}