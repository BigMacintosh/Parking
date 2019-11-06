using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{    
    public interface IGameLoop
    {
        bool Init(string[] args);
        void Shutdown();

        void Update();
        void FixedUpdate();
        void LateUpdate();
    }

    [DefaultExecutionOrder(-1000)]
    public class Game : MonoBehaviour
    {
        public bool IsHeadless { get; private set; }
        private List<IGameLoop> gameLoops = new List<IGameLoop>();
        private List<Type> requestedGameLoopTypes = new List<Type>();
        private List<string[]> requestedGameLoopArguments = new List<string[]>();

        public void RequestGameLoop(Type type, string[] args)
        {
            requestedGameLoopTypes.Add(type);
            requestedGameLoopArguments.Add(args);
        }

        private void ShutdownGameLoops()
        {
            foreach (var gameLoop in gameLoops)
                gameLoop.Shutdown();
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
                        initSucceeded = gameLoop.Init(requestedGameLoopArguments[i]);
                        if (!initSucceeded)
                            break;

                        gameLoops.Add(gameLoop);
                    }
                    catch (System.Exception e)
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

            try
            {
                if (!m_ErrorState)
                {
                    foreach (var gameLoop in gameLoops)
                    {
                        Debug.Log("Game: Update game loop");
                        gameLoop.Update();
                    }

//                levelManager.Update();
                }
            }
            catch (System.Exception e)
            {
//            HandleGameloopException(e);
                throw;
            }
        }

        bool m_ErrorState;

        private void FixedUpdate()
        {
            foreach (var gameLoop in gameLoops)
            {
                gameLoop.FixedUpdate();
            }
        }

        private void LateUpdate()
        {
            try
            {
                if (!m_ErrorState)
                {
                    foreach (var gameLoop in gameLoops)
                    {
                        gameLoop.LateUpdate();
                    }
                }
            }
            catch (System.Exception e)
            {
//            HandleGameloopException(e);
                throw;
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