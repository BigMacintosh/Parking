using Unity.Networking.Transport;

namespace Network.Events {
    public class ServerPing : Event {
        public int PingID { get; private set; }
        
        public ServerPing() {
            ID     = EventType.ServerPingEvent;
            Length = sizeof(byte) + sizeof(int);
        }

        public ServerPing(int pingID) {
            PingID = pingID;
        }

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(PingID);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            PingID = reader.ReadInt(ref context);
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}