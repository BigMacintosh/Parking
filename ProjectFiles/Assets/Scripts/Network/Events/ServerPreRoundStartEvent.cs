using System.Collections.Generic;
using Unity.Networking.Transport;

namespace Network.Events
{
    public class ServerPreRoundStartEvent : Event
    {
        public ushort RoundNumber { get; private set; }
        public ushort PreRoundLength { get; private set; }
        public ushort RoundLength { get; private set; }
        public ushort PlayerCount { get; private set; }
        public List<ushort> Spaces { get; private set; }

        public ServerPreRoundStartEvent()
        {
            ID = EventType.ServerPreRoundStartEvent;
            Length = 1;
        }

        public ServerPreRoundStartEvent(ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort playerCount, List<ushort> spaces) : this()
        {
            RoundNumber = roundNumber;
            PreRoundLength = preRoundLength;
            RoundLength = roundLength;
            PlayerCount = playerCount;
            Spaces = spaces;
            Length = sizeof(ushort) * (Spaces.Count + 5) + 1;
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write(RoundNumber);
            writer.Write(PreRoundLength);
            writer.Write(RoundLength);
            writer.Write(PlayerCount);
            writer.Write((ushort) Spaces.Count);
            foreach (var s in Spaces)
            {
                writer.Write(s);
            }
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            RoundNumber = reader.ReadUShort(ref context);
            PreRoundLength = reader.ReadUShort(ref context);
            RoundLength = reader.ReadUShort(ref context);
            PlayerCount = reader.ReadUShort(ref context);

            var spacesCount = reader.ReadUShort(ref context);
            Spaces = new List<ushort>();
            for (int i = 0; i < spacesCount; i++)
            {
                Spaces.Add(reader.ReadUShort(ref context));
            }
            Length = sizeof(ushort) * (Spaces.Count + 5) + 1;
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