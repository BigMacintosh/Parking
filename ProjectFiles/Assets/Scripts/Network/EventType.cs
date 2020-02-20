using System;

namespace Network
{
    [Flags]
    public enum EventType
    {
        Undefined                   = 0xFF,
        ClientHandshake             = 0x81,
        ClientLocationUpdate        = 0x82,
        ClientSpaceEvent            = 0x83,
        ServerHandshake             = 0x01,
        ServerLocationUpdate        = 0x02,
        ServerSpawnPlayerEvent      = 0x03,
        ServerPreRoundStartEvent    = 0x04,
        ServerRoundStartEvent       = 0x05,
        ServerRoundEndEvent         = 0x06,
        ServerEliminatePlayersEvent = 0x07,
        ServerDisconnectEvent       = 0x08,
        ServerGameEndEvent          = 0x09,
    }
}