using Unity.Networking.Transport;

namespace Network
{
    public static class NetworkExtensions
    {
        public static string IpAddress(this NetworkEndPoint endpoint)
        {
            /**
             * TODO: Find a better way of doing this. The NetworkEndPoint struct stores the socket data inside of an
             *  internal byte array, with the IP being stored at byte 4. This uses pointers to access bytes 4 - 8 and
             *  converts it into an IPv4 address.
             */
            unsafe
            {
                var rawBytes = ((byte*) &endpoint + 4);
                return $"{rawBytes[0]}.{rawBytes[1]}.{rawBytes[2]}.{rawBytes[3]}";
            }
        }
    }
}