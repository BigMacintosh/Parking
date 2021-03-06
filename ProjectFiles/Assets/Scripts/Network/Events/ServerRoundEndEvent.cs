﻿using Unity.Networking.Transport;

namespace Network.Events {
    // TODO: this event is exactly the same as ServerRoundStartEvent, might need merging?
    public class ServerRoundEndEvent : Event {
        public ServerRoundEndEvent() {
            ID     = EventType.ServerRoundEndEvent;
            Length = sizeof(byte) + sizeof(ushort);
        }

        public ServerRoundEndEvent(ushort roundNumber) : this() {
            RoundNumber = roundNumber;
        }

        public ushort RoundNumber { get; private set; }

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(RoundNumber);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            RoundNumber = reader.ReadUShort(ref context);
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}