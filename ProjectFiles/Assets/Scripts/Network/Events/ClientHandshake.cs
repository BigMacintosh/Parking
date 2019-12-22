using Unity.Networking.Transport;

namespace Network.Events
{
    public class ClientHandshake : Event
    {
        public ClientHandshake()
        {
            ID = 0x81;
        }
        
        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            // ClientHandshake is empty, no need to deserialise
        }
    }
}