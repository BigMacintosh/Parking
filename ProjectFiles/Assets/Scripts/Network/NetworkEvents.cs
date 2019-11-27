using System;
using Unity.Collections;
using Unity.Networking.Transport;

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



    public static class NetworkEventExtension
    {
        public static DataStreamWriter GetWriter(this ClientNetworkEvent ev, int capacity, Allocator allocator)
        {
            var writer = new DataStreamWriter(capacity + 1, allocator);
            writer.Write((byte) ev);
            return writer;
        }
        
        public static DataStreamWriter GetWriter(this ServerNetworkEvent ev, int capacity, Allocator allocator)
        {
            var writer = new DataStreamWriter(capacity + 1, allocator);
            writer.Write((byte) ev);
            return writer;
        }
    }
    
}