using System;

namespace Network
{
    [Serializable]
    public class ServerConfig
    {
        public string IpAddress;
        public ushort Port;
        public ushort MaxPlayers;

        public static ServerConfig DefaultConfig = new ServerConfig
        {
            IpAddress = "0.0.0.0",
            Port = 25565,
            MaxPlayers = 64
        };
    }
}