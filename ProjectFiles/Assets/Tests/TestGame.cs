using System;
using System.Collections.Generic;
using Game.Core.Rounds;
using Game.Main;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {
    public class TestGame : MonoBehaviour, IMonoBehaviourTest {
        public bool IsTestFinished => gameLoop.TestFinished;

        // Private Fields
        private TestGameLoop gameLoop;

        private void Awake() {
            RoundProperties.MaxRounds      = 1;
            RoundProperties.FreeroamLength = 1;
            RoundProperties.PreRoundLength = 1;
            RoundProperties.RoundLength    = 3;

            try {
                gameLoop = (TestGameLoop) Activator.CreateInstance(typeof(TestGameLoop));
                var initSucceeded = gameLoop.Init(new string[0]);
                if (!initSucceeded) {
                    Debug.Log("Game loop failed to initialise.");
                }
            } catch (Exception e) {
                Debug.Log($"Game loop initialization threw exception : ({e.Message})\n{e.StackTrace}");
            }
        }

        // Update Loops
        public void Update() {
            gameLoop.Update();
        }
    }
}