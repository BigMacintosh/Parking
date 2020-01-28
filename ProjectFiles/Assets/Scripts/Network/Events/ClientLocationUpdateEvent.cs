using Game;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events
{
    public class ClientLocationUpdateEvent : Event
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Vector3 Velocity { get; private set; }
        public Vector3 AngularVelocity { get; private set; }

        public ClientLocationUpdateEvent()
        {
            ID = EventType.ClientLocationUpdate;
            Length = (13 * sizeof(float)) + 1;
        }

        public ClientLocationUpdateEvent(World world) : this()
        {
            var transform = world.GetPlayerTransform(world.ClientID);
            Position = transform.position;
            Rotation = transform.rotation;
            Velocity = world.GetPlayerVelocity(world.ClientID);
            AngularVelocity = world.GetPlayerAngularVelocity(world.ClientID);
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.WriteVector3(Position);
            writer.WriteQuaternion(Rotation);
            writer.WriteVector3(Velocity);
            writer.WriteVector3(AngularVelocity);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            Position = reader.ReadVector3(ref context);
            Rotation = reader.ReadQuaternion(ref context);
            Velocity = reader.ReadVector3(ref context);
            AngularVelocity = reader.ReadVector3(ref context);
        }
    }
}