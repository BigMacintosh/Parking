using Unity.Networking.Transport;

namespace Network.Events {
    public class ServerDisconnectEvent : Event {
        public ServerDisconnectEvent() {
            ID     = EventType.ServerDisconnectEvent;
            Length = sizeof(ushort) + sizeof(byte);
        }

        public ServerDisconnectEvent(ushort playerID) : this() {
            PlayerID = playerID;
        }

        public ushort PlayerID { get; private set; }

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(PlayerID);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            PlayerID = reader.ReadUShort(ref context);
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}