using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{    
    public interface IGameLoop
    {
        bool Init(Spawner spawner, string[] args);
        void Shutdown();

        void Update();
        void FixedUpdate();
        void LateUpdate();
    }

    [DefaultExecutionOrder(-1000)]
    public class Game : MonoBehaviour
    {
        public bool IsHeadless { get; private set; }
        private readonly List<IGameLoop> gameLoops = new List<IGameLoop>();
        private readonly List<Type> requestedGameLoopTypes = new List<Type>();
        private readonly List<string[]> requestedGameLoopArguments = new List<string[]>();

        [SerializeField] private Spawner spawner;

        public void RequestGameLoop(Type type, string[] args)
        {
            requestedGameLoopTypes.Add(type);
            requestedGameLoopArguments.Add(args);
        }

        private void ShutdownGameLoops()
        {
            foreach (var gameLoop in gameLoops)
            {
                gameLoop.Shutdown();
            }

            gameLoops.Clear();
        }

        private void Awake()
        {
            var commandLineArgs = new List<string>(Environment.GetCommandLineArgs());
            IsHeadless = commandLineArgs.Contains("-batchmode");
            Debug.Log(IsHeadless);

            if (IsHeadless)
            {
                RequestGameLoop(typeof(ServerGameLoop), new string[0]);
            }
            else
            {
#if UNITY_EDITOR
                RequestGameLoop(typeof(ServerGameLoop), new string[0]);
#endif
                RequestGameLoop(typeof(ClientGameLoop), new string[0]);
            }

            Debug.Log(requestedGameLoopTypes.Count);
            // Inititalise level manager
        }

        public void Update()
        {
            // Switch game loop if needed
            if (requestedGameLoopTypes.Count > 0)
            {
                // Multiple running gameloops only allowed in editor
#if !UNITY_EDITOR
            ShutdownGameLoops();
#endif
                bool initSucceeded = false;
                for (int i = 0; i < requestedGameLoopTypes.Count; i++)
                {
                    try
                    {
                        IGameLoop gameLoop = (IGameLoop) System.Activator.CreateInstance(requestedGameLoopTypes[i]);
                        initSucceeded = gameLoop.Init(spawner, requestedGameLoopArguments[i]);
                        if (!initSucceeded)
                        {
                            Debug.Log("Game loop failed to initialise.");
                            break;
                        }

                        gameLoops.Add(gameLoop);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(string.Format("Game loop initialization threw exception : ({0})\n{1}", e.Message,
                            e.StackTrace));
                    }
                }


                if (!initSucceeded)
                {
                    ShutdownGameLoops();

                    Debug.Log("Game loop initialization failed ... reverting to boot loop");
                }

                requestedGameLoopTypes.Clear();
                requestedGameLoopArguments.Clear();
            }
            
            foreach (var gameLoop in gameLoops)
            {
                gameLoop.Update();
            }

        }

        private void FixedUpdate()
        {
            foreach (var gameLoop in gameLoops)
            {
                gameLoop.FixedUpdate();
            }
        }

        private void LateUpdate()
        {
            foreach (var gameLoop in gameLoops)
            {
                gameLoop.LateUpdate();
            }
        }

        private void OnApplicationQuit()
        {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
            ShutdownGameLoops();
        }

    }
}