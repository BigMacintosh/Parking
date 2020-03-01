using Unity.Networking.Transport;

namespace Network.Events {
    public class AdminClientStartGameEvent : Event {
        public AdminClientStartGameEvent() {
            ID     = EventType.AdminClientStartGameEvent;
            Length = sizeof(byte);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            // nothing to deserialise.
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}