using Game;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events
{
    public class ServerHandshakeEvent : Event
    {
        public ushort PlayerID { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        
        public ServerHandshakeEvent()
        {
            ID = EventType.ServerHandshake;
            Length = ((3 + 4) * sizeof(float)) + 2;
        }

        public ServerHandshakeEvent(Transform transform, int playerID) : this()
        {
            Position = transform.position;
            Rotation = transform.rotation;
            PlayerID = (ushort) playerID;
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write(PlayerID);
            writer.WriteVector3(Position);
            writer.WriteQuaternion(Rotation);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            PlayerID = reader.ReadUShort(ref context);
            Position = reader.ReadVector3(ref context);
            Rotation = reader.ReadQuaternion(ref context);
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