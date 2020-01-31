﻿using Unity.Networking.Transport;

namespace Network.Events
{
    public class ClientSpaceEvent : Event
    {
        public ushort RoundNumber { get; private set; }
        public ushort SpaceID { get; private set; }

        public ClientSpaceEvent()
        {
            ID = EventType.ClientSpaceEvent;
            Length = sizeof(byte) + sizeof(ushort) * 2;
        }

        public ClientSpaceEvent(byte roundNumber, byte spaceId)
        {
            RoundNumber = roundNumber;
            SpaceID = spaceId;
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write(RoundNumber);
            writer.Write(SpaceID);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            RoundNumber = reader.ReadUShort(ref context);
            SpaceID = reader.ReadUShort(ref context);
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