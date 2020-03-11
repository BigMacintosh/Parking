using Unity.Networking.Transport;

namespace Network.Events {
    public class ServerPing : Event {
        public ServerPing() {
            ID     = EventType.ServerPingEvent;
            Length = 1;
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            // ServerKeepAlive is empty, no need to deserialise
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}