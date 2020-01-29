using System.Collections.Generic;
using Unity.Networking.Transport;

namespace Network.Events
{
    public class ServerEliminatePlayersEvent : Event
    {
        public byte RoundNumber { get; private set; }
        public List<byte> Players { get; private set; }

        public ServerEliminatePlayersEvent()
        {
            ID = EventType.ServerEliminatePlayersEvent;
            Length = 1;
        }

        public ServerEliminatePlayersEvent(byte roundNumber, List<byte> players)
        {
            RoundNumber = roundNumber;
            Players = players;
            Length = sizeof(byte) * (Players.Count + 2);
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write(RoundNumber);
            writer.Write((byte) Players.Count);
            foreach (var p in Players)
            {
                writer.Write(p);
            }
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            RoundNumber = reader.ReadByte(ref context);

            var playerCount = reader.ReadByte(ref context);
            Players = new List<byte>();
            for (int i = 0; i < playerCount; i++)
            {
                Players.Add(reader.ReadByte(ref context));
            }
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