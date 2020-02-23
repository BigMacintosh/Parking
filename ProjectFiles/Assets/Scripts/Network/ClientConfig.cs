using UnityEngine;

namespace Network
{
    public enum VehicleType
    {
        Hatchback
    }

    public enum GameMode
    {
        PlayerMode = 0x01,
        AdminMode  = 0x02,
    }
    
    public static class ClientConfig
    {
        public static string PlayerName = "";
#if UNITY_EDITOR
        public static string ServerIP = "127.0.0.1";
#else
        public static string ServerIP = "35.177.253.83";
#endif
        public static VehicleType VehicleType = VehicleType.Hatchback;
        public static Color VehicleColour = Color.red;
        public static GameMode GameMode = GameMode.PlayerMode;
    }
}