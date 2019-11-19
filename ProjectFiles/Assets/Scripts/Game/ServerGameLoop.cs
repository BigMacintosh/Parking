using System;
using System.IO;
using Network;
using UnityEngine;

namespace Game
{
    public class ServerGameLoop : IGameLoop
    {
        private Server server;
        private World world;

        public bool Init(string[] args)
        {
            // TODO: make the config path an argument so we can swap configs later?
            var config = LoadConfigOrDefault("server-config.json");
            
            // Create world
            world = new World();

            // Start server
            server = new Server(world, config);
            var success = server.Start();
            
            return success;
        }

        private ServerConfig LoadConfigOrDefault(string path)
        {
            try
            {
                using (var reader = new StreamReader(path))
                {
                    var json = reader.ReadToEnd();
                    return JsonUtility.FromJson<ServerConfig>(json);
                }
            }
            catch (Exception e) when (e is ArgumentException || e is FileNotFoundException)
            {
                Debug.Log(e);
                Debug.Log($"Failed to load {path}. Generating and running default config at server-config.json...");
                SaveConfig(path, ServerConfig.DefaultConfig);
                return ServerConfig.DefaultConfig;
            }
        }

        private void SaveConfig(string path, ServerConfig config)
        {
            using (var writer = new StreamWriter(path))
            {
                var json = JsonUtility.ToJson(config);
                writer.Write(json);
            }
        }
        
        public void Shutdown()
        {
            server.Shutdown();
//          Destroy the world here.
        }

        public void Update()
        {
            server.HandleNetworkEvents();
            world.Update();
        }

        public void FixedUpdate()
        {
            // Trigger network send.
        }

        public void LateUpdate()
        {
            // Nothing required here yet.
        }
    }
}
