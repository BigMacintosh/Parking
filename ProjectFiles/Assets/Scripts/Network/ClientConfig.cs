using Game.Entity;
using UnityEngine;

namespace Network {
    public enum GameMode {
        PlayerMode = 0x01,
        AdminMode  = 0x02
    }

    public static class ClientConfig {
        public static string   PlayerName    = "";
        public static CarType  VehicleType   = CarType.Hatchback;
        public static Color    VehicleColour = Color.red;
        public static GameMode GameMode      = GameMode.PlayerMode;
        public static int      PlayerID => Me?.PlayerID ?? -1;
        public static Player   Me;

    #if UNITY_EDITOR
        public static string ServerIP = "127.0.0.1";
    #else
        public static string ServerIP = "35.177.253.83";
    #endif
    }
}