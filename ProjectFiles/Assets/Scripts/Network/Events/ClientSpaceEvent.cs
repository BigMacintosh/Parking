using Unity.Networking.Transport;

namespace Network.Events
{
    public class ClientSpaceEvent : Event
    {
        public byte RoundNumber { get; private set; }
        public byte SpaceID { get; private set; }

        public ClientSpaceEvent()
        {
            ID = EventType.ClientSpaceEvent;
            Length = 3;
        }

        public ClientSpaceEvent(byte roundNumber, byte spaceId)
        {
            RoundNumber = roundNumber;
            SpaceID = spaceId;
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write(RoundNumber);
            writer.Write(SpaceID);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            RoundNumber = reader.ReadByte(ref context);
            SpaceID = reader.ReadByte(ref context);
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