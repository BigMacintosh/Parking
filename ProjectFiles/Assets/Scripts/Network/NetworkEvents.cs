using System;
using System.Collections.Generic;
using System.Linq;

namespace Network
{
    [Flags]
    public enum ClientNetworkEvent
    {
        ClientHandshake      = 0x81,
        ClientLocationUpdate = 0x82,
    }

    public enum ServerNetworkEvent
    {
        ServerHandshake      = 0x01,
        ServerLocationUpdate = 0x02,
    }
}