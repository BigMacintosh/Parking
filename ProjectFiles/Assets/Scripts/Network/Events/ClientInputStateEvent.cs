using Game.Core.Driving;
using Unity.Networking.Transport;

namespace Network.Events {
    public class ClientInputStateEvent : Event {
        public ulong Tick { get; private set; }
        public VehicleInputState Inputs { get; private set; }
        
        public ClientInputStateEvent() {
            ID = EventType.ClientInputStateEvent;
            Length = sizeof(byte) + sizeof(ulong) + sizeof(float) * 4;
        }

        public ClientInputStateEvent(ulong tick, VehicleInputState inputs) : this() {
            Tick = tick;
            Inputs = inputs;
        }

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(Tick);
            writer.Write(Inputs.Drive);
            writer.Write(Inputs.Turn);
            writer.Write(Inputs.Jump);
            writer.Write(Inputs.Drift);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            Tick = reader.ReadULong(ref context);
            Inputs = new VehicleInputState {
                Drive = reader.ReadFloat(ref context),
                Turn  = reader.ReadFloat(ref context),
                Jump  = reader.ReadFloat(ref context),
                Drift = reader.ReadFloat(ref context)
            };
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}