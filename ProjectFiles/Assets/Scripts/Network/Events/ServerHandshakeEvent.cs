using Game;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events
{
    public class ServerHandshakeEvent : Event
    {
        public int PlayerID { get; private set; }
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
            PlayerID = playerID;
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write((byte) PlayerID);
            writer.WriteVector3(Position);
            writer.WriteQuaternion(Rotation);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            PlayerID = reader.ReadByte(ref context);
            Position = reader.ReadVector3(ref context);
            Rotation = reader.ReadQuaternion(ref context);
        }
    }
}