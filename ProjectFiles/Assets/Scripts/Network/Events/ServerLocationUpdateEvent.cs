using System.Collections.Generic;
using Game;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events
{
    public class ServerLocationUpdateEvent : Event
    {
        public Dictionary<int, Vector3> Positions { get; } = new Dictionary<int, Vector3>();
        public Dictionary<int, Quaternion> Rotations { get; } = new Dictionary<int, Quaternion>();
        public Dictionary<int, Vector3> Velocities { get; } = new Dictionary<int, Vector3>();
        public Dictionary<int, Vector3> AngularVelocities { get; } = new Dictionary<int, Vector3>();
        private int count;

        public ServerLocationUpdateEvent()
        {
            ID = EventType.ServerLocationUpdate;
            Length = 1;
        }

        public ServerLocationUpdateEvent(World world) : this()
        {
            Length = (sizeof(ushort) + (1 + 3 + 4 + 3 + 3) * sizeof(float)) * world.GetNumPlayers() + sizeof(ushort) + sizeof(byte);
            foreach (var pair in world.Players)
            {
                var id = pair.Key;
                var car = pair.Value;
                
                var transform = car.transform;
                Positions[id] = transform.position;
                Rotations[id] = transform.rotation;

                var rb = car.GetComponent<Rigidbody>();
                Velocities[id] = rb.velocity;
                AngularVelocities[id] = rb.angularVelocity;
            }
        }
        
        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write((ushort) Positions.Count);
            foreach (var id in Positions.Keys)
            {
                writer.Write((ushort) id);
                writer.WriteVector3(Positions[id]);
                writer.WriteQuaternion(Rotations[id]);
                writer.WriteVector3(Velocities[id]);
                writer.WriteVector3(AngularVelocities[id]);
            }
        }
        
        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            var length = reader.ReadUShort(ref context);
            for (int i = 0; i < length; i++)
            {
                var id = reader.ReadUShort(ref context);
                Positions[id] = reader.ReadVector3(ref context);
                Rotations[id] = reader.ReadQuaternion(ref context);
                Velocities[id] = reader.ReadVector3(ref context);
                AngularVelocities[id] = reader.ReadVector3(ref context);
            }
            Length = (sizeof(ushort) + (1 + 3 + 4 + 3 + 3) * sizeof(float)) * length + sizeof(ushort) + sizeof(byte);
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