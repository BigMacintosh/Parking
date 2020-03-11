using Unity.Networking.Transport;

namespace Network.Events {
    public class ClientPing : Event {
        public ClientPing() {
            ID     = EventType.ServerPingEvent;
            Length = 1;
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}