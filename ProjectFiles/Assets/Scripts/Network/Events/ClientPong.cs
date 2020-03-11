using Unity.Networking.Transport;

namespace Network.Events {
    public class ClientPong : Event {
        
        public int PongID { get; private set; }
        
        public ClientPong() {
            ID     = EventType.ClientPongEvent;
            Length = sizeof(byte) + sizeof(int);
        }
        
        public ClientPong(int pongID) {
            PongID = pongID;
        }

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(PongID);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            PongID = reader.ReadInt(ref context);
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}