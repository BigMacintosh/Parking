using Unity.Networking.Transport;

namespace Network.Events {
    public class ClientSpaceExitEvent : Event {
        public ClientSpaceExitEvent() {
            ID     = EventType.ClientSpaceExitEvent;
            Length = sizeof(byte) + sizeof(ushort);
        }

        public ClientSpaceExitEvent(ushort spaceId) : this() {
            SpaceID = spaceId;
        }

        public ushort SpaceID { get; private set; }

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(SpaceID);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            SpaceID = reader.ReadUShort(ref context);
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}