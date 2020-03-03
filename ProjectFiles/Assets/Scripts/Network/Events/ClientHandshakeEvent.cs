using Game.Entity;
using Unity.Networking.Transport;

namespace Network.Events {
    public class ClientHandshakeEvent : Event {
        public GameMode GameMode { get; private set; }
        public PlayerOptions PlayerOptions { get; private set; }
        
        public ClientHandshakeEvent() {
            ID     = EventType.ClientHandshake;
        }

        public ClientHandshakeEvent(GameMode gameMode, PlayerOptions playerOptions) : this() {
            GameMode = gameMode;
            PlayerOptions = playerOptions;
            Length = sizeof(byte) * 2 + playerOptions.WriterLength();
        }

        

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write((byte) GameMode);
            writer.WritePlayerOptions(PlayerOptions);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            GameMode = (GameMode) reader.ReadByte(ref context);
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