using System;
using Game.Core.Parking;
using Game.Core.Rounds;
using Game.Entity;
using Game.Main;
using NUnit.Framework;
using Utils;


namespace Tests {
    public class TestGameLoop : IGameLoop {
        // Fields
        private ServerParkingSpaceManager parkingSpaceManager;
        private RoundManager              roundManager;
        private ServerWorld               world;

        private Timer timer;
        public  bool  TestFinished;

        public void Shutdown() {
            throw new NotImplementedException();
        }

        public void Update() {
            roundManager.Update();
            parkingSpaceManager.Update();
            timer.Update();
        }

        public void FixedUpdate() {
            throw new NotImplementedException();
        }

        public void LateUpdate() {
            throw new NotImplementedException();
        }

        public bool Init(string[] args) {
            timer = new Timer(0);

            TestFinished = false;

            // Initialise Test components
            parkingSpaceManager = new ServerParkingSpaceManager();

            for (int i = 0; i < 10; i++) {
                parkingSpaceManager.TEST_ONLY_AddParkingSpace(TestUtils.NewMockParkingSpaceController(i));
            }

            world        = new ServerWorld(parkingSpaceManager);
            roundManager = new RoundManager(world, parkingSpaceManager);

            for (int i = 0; i < 3; i++) {
                world.AddPlayer(TestUtils.NewMockPlayer(i));
            }

            // Set up some standard event listeners (as in the ServerGameLoop)
            roundManager.GameStartEvent  += (length, players) => world.SpawnPlayers();
            roundManager.RoundEndEvent   += parkingSpaceManager.OnRoundEnd;

            // Schedule a SpaceEnter event
            roundManager.RoundStartEvent += (number, active) => {
                parkingSpaceManager.OnRoundStart(number, active);
                timer         =  new Timer(0.5f);
                timer.Elapsed += () => parkingSpaceManager.OnSpaceEnter(1, active[0]);
                timer.Start();
            };

            roundManager.GameEndEvent += winners => {
                Assert.True(winners.Count == 1);
                Assert.True(winners.Contains(1));

                winners.ForEach(world.DestroyPlayer);
                TestFinished = true;
            };

            roundManager.StartGame();
            return true;
        }
    }
}