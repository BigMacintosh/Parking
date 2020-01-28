using Unity.Networking.Transport;

namespace Network.Events
{
    public class ClientHandshakeEvent : Event
    {
        public ClientHandshakeEvent()
        {
            ID = EventType.ClientHandshake;
            Length = 1;
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            // ClientHandshake is empty, no need to deserialise
        }
    }
}