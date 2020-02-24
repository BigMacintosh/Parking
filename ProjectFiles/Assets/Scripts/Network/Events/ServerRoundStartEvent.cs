using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events
{
    public class ServerRoundStartEvent : Event
    {
        public ushort RoundNumber { get; private set; }
        public List<ushort> Spaces { get; private set; }

        public ServerRoundStartEvent()
        {
            ID = EventType.ServerRoundStartEvent;
            Length = 1;
        }

        public ServerRoundStartEvent(ushort roundNumber, List<ushort> spaces) : this()
        {
            RoundNumber = roundNumber;
            Spaces = spaces;
            Length = sizeof(ushort) * (Spaces.Count + 2) + sizeof(byte);
            Debug.Log("HERE 123456789 " + Spaces.Count);
        }
        
        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write(RoundNumber);
            writer.Write((ushort) Spaces.Count);
            foreach (var s in Spaces)
            {
                writer.Write(s);
            }
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            RoundNumber = reader.ReadUShort(ref context);
            var spacesCount = reader.ReadUShort(ref context);
            Spaces = new List<ushort>();
            for (int i = 0; i < spacesCount; i++)
            {
                Spaces.Add(reader.ReadUShort(ref context));
            }
            Length = sizeof(ushort) * (Spaces.Count + 2) + sizeof(byte);
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
