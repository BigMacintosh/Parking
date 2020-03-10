using System;

namespace Network {
    [Flags]
    public enum EventType {
        Undefined = 0xFF,

        ClientHandshake       = 0x81,
        ClientLocationUpdate  = 0x82,
        ClientSpaceEnterEvent = 0x83,
        ClientSpaceExitEvent  = 0x84,
        ClientInputStateEvent = 0x85,
        
        AdminClientStartGameEvent = 0xA0,

        ServerHandshake               = 0x01,
        ServerLocationUpdate          = 0x02,
        ServerSpawnPlayerEvent        = 0x03,
        ServerPreRoundStartEvent      = 0x04,
        ServerRoundStartEvent         = 0x05,
        ServerRoundEndEvent           = 0x06,
        ServerEliminatePlayersEvent   = 0x07,
        ServerDisconnectEvent         = 0x08,
        ServerGameEndEvent            = 0x09,
        ServerGameStartEvent          = 0x0B,
        ServerKeepAliveEvent          = 0x0A,
        ServerSpaceClaimedEvent       = 0x0C,
        ServerNewPlayerConnectedEvent = 0x0D,
    }
}