using Unity.Networking.Transport;

namespace Network.Events
{
    public class ClientHandshakeEvent : Event
    {
        public GameMode GameMode { get; private set; }

        public ClientHandshakeEvent()
        {
            ID = EventType.ClientHandshake;
            Length = sizeof(byte) * 2;
        }
        
        public ClientHandshakeEvent(GameMode gameMode) : this()
        {
            GameMode = gameMode;
        }
        
        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write((byte) GameMode);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            GameMode = (GameMode) reader.ReadByte(ref context);
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