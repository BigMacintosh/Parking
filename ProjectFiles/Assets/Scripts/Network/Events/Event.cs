using Unity.Networking.Transport;

namespace Network.Events {
    public abstract class Event {
        protected EventType ID = EventType.Undefined;
        public    int       Length { get; protected set; }
        
        public virtual void Serialise(DataStreamWriter writer) {
            writer.Write((byte) ID);
        }

        public abstract void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context);

        public abstract void Handle(Server server, NetworkConnection connection);

        public abstract void Handle(Client client, NetworkConnection connection);

        public override string ToString() {
            return $"Event[{ID.ToString()}]";
        }
    }
}