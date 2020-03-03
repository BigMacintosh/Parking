using Game.Entity;
using Unity.Networking.Transport;

namespace Network.Events {
    public class ServerNewPlayerConnectedEvent : Event {
        public int PlayerID { get; private set; }
        public PlayerOptions PlayerOptions { get; private set; }
        
        public ServerNewPlayerConnectedEvent() {
            ID     = EventType.ServerNewPlayerConnectedEvent;
        }

        public ServerNewPlayerConnectedEvent(int playerID, PlayerOptions playerOptions) : this() {
            PlayerID = playerID;
            PlayerOptions = playerOptions;
            Length = sizeof(byte) + sizeof(int) + playerOptions.WriterLength();
        }

        

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(PlayerID);
            writer.WritePlayerOptions(PlayerOptions);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            PlayerID = reader.ReadInt(ref context);
            PlayerOptions = reader.ReadPlayerOptions(ref context);
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}