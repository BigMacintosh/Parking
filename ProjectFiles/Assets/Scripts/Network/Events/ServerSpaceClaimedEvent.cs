using Unity.Networking.Transport;

namespace Network.Events
{
    public class ServerSpaceClaimedEvent : Event
    {
        public ushort SpaceID { get; private set; }
        public int PlayerID { get; private set; }

        public ServerSpaceClaimedEvent()
        {
            ID = EventType.ClientSpaceExitEvent;    
            Length = sizeof(byte) + sizeof(int) + sizeof(ushort);
        }

        public ServerSpaceClaimedEvent(int playerID, ushort spaceID) : this()
        {
            PlayerID = playerID;
            SpaceID = spaceID;
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write(PlayerID);
            writer.Write(SpaceID);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            PlayerID = reader.ReadInt(ref context);
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