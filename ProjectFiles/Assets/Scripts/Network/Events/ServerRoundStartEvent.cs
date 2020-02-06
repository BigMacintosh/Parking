using Unity.Networking.Transport;

namespace Network.Events
{
    public class ServerRoundStartEvent : Event
    {
        public ushort RoundNumber { get; private set; }

        public ServerRoundStartEvent()
        {
            ID = EventType.ServerRoundStartEvent;
            Length = sizeof(byte) + sizeof(ushort);
        }

        public ServerRoundStartEvent(ushort roundNumber) : this()
        {
            RoundNumber = roundNumber;
        }
        
        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write(RoundNumber);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            RoundNumber = reader.ReadUShort(ref context);
        }

        public override void Handle(Server server, NetworkConnection connection)
        {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection)
        {
            client.Handle(this, connection);
        }
    }
}
