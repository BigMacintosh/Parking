using System.Collections.Generic;
using Unity.Networking.Transport;

namespace Network.Events
{
    public class ServerPreRoundStartEvent : Event
    {
        public byte RoundNumber { get; private set; }
        // TODO: not sure on the type for the round lengths - it's long for now just in case it's big
        public ulong PreRoundLength { get; private set; }
        public ulong RoundLength { get; private set; }
        public byte PlayerCount { get; private set; }
        public List<byte> Spaces { get; private set; }

        public ServerPreRoundStartEvent()
        {
            ID = EventType.ServerPreRoundStartEvent;
            Length = 1;
        }

        public ServerPreRoundStartEvent(byte roundNumber, ulong preRoundLength, ulong roundLength, byte playerCount, List<byte> spaces) : this()
        {
            RoundNumber = roundNumber;
            PreRoundLength = preRoundLength;
            RoundLength = roundLength;
            PlayerCount = playerCount;
            Spaces = spaces;
            Length = (sizeof(byte) * Spaces.Count) + (sizeof(ulong) * 2) + (sizeof(byte) * 4);
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write(RoundNumber);
            writer.Write(PreRoundLength);
            writer.Write(RoundLength);
            writer.Write(PlayerCount);
            writer.Write((byte) Spaces.Count);
            foreach (var s in Spaces)
            {
                writer.Write(s);
            }
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            RoundNumber = reader.ReadByte(ref context);
            PreRoundLength = reader.ReadULong(ref context);
            RoundLength = reader.ReadULong(ref context);
            PlayerCount = reader.ReadByte(ref context);

            var spacesCount = reader.ReadByte(ref context);
            Spaces = new List<byte>();
            for (int i = 0; i < spacesCount; i++)
            {
                Spaces.Add(reader.ReadByte(ref context));
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