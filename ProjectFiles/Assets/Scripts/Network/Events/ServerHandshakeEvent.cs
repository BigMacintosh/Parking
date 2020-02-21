using Game;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events
{
    public class ServerHandshakeEvent : Event
    {
        public int PlayerID { get; private set; }

        public ServerHandshakeEvent()
        {
            ID = EventType.ServerHandshake;
            Length = sizeof(int) + sizeof(byte);
        }

        public ServerHandshakeEvent(int playerID) : this()
        {
            PlayerID = playerID;
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write(PlayerID);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            PlayerID = reader.ReadInt(ref context);
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