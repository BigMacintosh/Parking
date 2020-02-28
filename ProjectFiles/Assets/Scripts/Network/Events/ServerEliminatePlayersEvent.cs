using System.Collections.Generic;
using Unity.Networking.Transport;

namespace Network.Events {
    public class ServerEliminatePlayersEvent : Event {
        public ServerEliminatePlayersEvent() {
            ID = EventType.ServerEliminatePlayersEvent;
        }

        public ServerEliminatePlayersEvent(ushort roundNumber, List<int> players) : this() {
            RoundNumber = roundNumber;
            Players     = players;
            Length      = 2 * sizeof(ushort) + Players.Count * sizeof(int) + sizeof(byte);
        }

        public ushort    RoundNumber { get; private set; }
        public List<int> Players     { get; private set; }

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(RoundNumber);
            writer.Write((ushort) Players.Count);
            foreach (var p in Players) {
                writer.Write(p);
            }
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            RoundNumber = reader.ReadByte(ref context);

            var playerCount = reader.ReadByte(ref context);
            Players = new List<int>();
            for (var i = 0; i < playerCount; i++) {
                Players.Add(reader.ReadByte(ref context));
            }

            Length = sizeof(ushort) * (Players.Count + 2);
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}