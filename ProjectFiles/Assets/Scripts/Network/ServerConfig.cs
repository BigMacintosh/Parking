using System;
using System.IO;
using UnityEngine;

namespace Network {
    // [Serializable]
    public class ServerConfig {
        private static readonly ServerConfig DefaultConfig = new ServerConfig {
            IpAddress  = "0.0.0.0",
            Port       = 25565,
            MaxPlayers = 64
        };

        public string IpAddress;
        public ushort MaxPlayers;
        public ushort Port;

        public static ServerConfig LoadConfigOrDefault(string path) {
            try {
                using (var reader = new StreamReader(path)) {
                    var json = reader.ReadToEnd();
                    var cfg  = JsonUtility.FromJson<ServerConfig>(json);
                    Debug.Log($"Server: Successfully loaded config from {path}...");
                    return cfg;
                }
            } catch (Exception e) when (e is ArgumentException || e is FileNotFoundException) {
                Debug.Log(e);
                Debug.Log($"Failed to load {path}. Generating and running default config at server-config.json...");
                DefaultConfig.SaveConfig("server-config.json");
                return DefaultConfig;
            }
        }

        private void SaveConfig(string path) {
            using (var writer = new StreamWriter(path)) {
                var json = JsonUtility.ToJson(this, true);
                writer.Write(json);
            }
        }
    }
}