using System;
using Unity.Collections;
using Unity.Networking.Transport;

namespace Network
{
    [Flags]
    public enum EventType
    {
        Undefined              = 0xFF,
        ClientHandshake        = 0x81,
        ClientLocationUpdate   = 0x82,
        ServerHandshake        = 0x01,
        ServerLocationUpdate   = 0x02,
        ServerSpawnPlayerEvent = 0x03,
    }
}