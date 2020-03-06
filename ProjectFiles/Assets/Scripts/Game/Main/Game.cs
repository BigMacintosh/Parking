using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Main {
    public interface IGameLoop {
        bool Init(string[] args);
        void Shutdown();
        void Update();
        void FixedUpdate();
        void LateUpdate();
    }

    public enum ServerClientSetting {
        Client,
        Server,
        Standalone
    }

    [DefaultExecutionOrder(-1000)]
    public class Game : MonoBehaviour {
        // Serialized Fields
        [SerializeField] private ServerClientSetting gameMode       = ServerClientSetting.Standalone;

        // Private Fields
        private readonly List<IGameLoop> gameLoops                  = new List<IGameLoop>();
        private readonly List<string[]>  requestedGameLoopArguments = new List<string[]>();
        private readonly List<Type>      requestedGameLoopTypes     = new List<Type>();


        private void RequestGameLoop(Type type, string[] args) {
            requestedGameLoopTypes.Add(type);
            requestedGameLoopArguments.Add(args);
        }

        private void ShutdownGameLoops() {
            foreach (var gameLoop in gameLoops) {
                gameLoop.Shutdown();
            }

            gameLoops.Clear();
        }

        private void Awake() {
        #if UNITY_EDITOR
            if (gameMode == ServerClientSetting.Standalone) {
                RequestGameLoop(typeof(ClientGameLoop), new[] {"standalone"});
            } else if (gameMode == ServerClientSetting.Server) {
                RequestGameLoop(typeof(ServerGameLoop), new string[0]);
            } else if (gameMode == ServerClientSetting.Client) {
                RequestGameLoop(typeof(ClientGameLoop), new string[0]);
            }
        #elif UNITY_STANDALONE_LINUX && UNITY_SERVER
            RequestGameLoop(typeof(ServerGameLoop), new string[0]);
        #elif UNITY_STANDALONE && !UNITY_SERVER
            RequestGameLoop(typeof(ClientGameLoop), new string[0]);
        #endif
        }


        // Update Loops
        public void Update() {

            // Switch game loop if needed
            if (requestedGameLoopTypes.Count > 0) {
                var initSucceeded = false;
                for (var i = 0; i < requestedGameLoopTypes.Count; i++) {
                    try {
                        var gameLoop = (IGameLoop) Activator.CreateInstance(requestedGameLoopTypes[i]);
                        initSucceeded = gameLoop.Init(requestedGameLoopArguments[i]);
                        if (!initSucceeded) {
                            Debug.Log("Game loop failed to initialise.");
                            break;
                        }

                        gameLoops.Add(gameLoop);
                    } catch (Exception e) {
                        Debug.Log($"Game loop initialization threw exception : ({e.Message})\n{e.StackTrace}");
                    }
                }


                if (!initSucceeded) {
                    ShutdownGameLoops();

                    Debug.Log("Game loop initialization failed.");
                }

                requestedGameLoopTypes.Clear();
                requestedGameLoopArguments.Clear();
            }

            foreach (var gameLoop in gameLoops) {
                gameLoop.Update();
            }
        }

        private void FixedUpdate() {
            foreach (var gameLoop in gameLoops) {
                gameLoop.FixedUpdate();
            }
        }

        private void LateUpdate() {
            foreach (var gameLoop in gameLoops) {
                gameLoop.LateUpdate();
            }
        }

        private void OnApplicationQuit() {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        #endif
            ShutdownGameLoops();
        }
    }
}
