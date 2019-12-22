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

        public ServerLocationUpdateEvent() {}
        
        public ServerLocationUpdateEvent(World world)
        {
            ID = 0x02;
            Length = ((3 + 4 + 3 + 3) * sizeof(float)) * world.GetNumPlayers() + 1;
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
            writer.Write(Positions.Count);
            foreach (var id in Positions.Keys)
            {
                writer.WriteVector3(Positions[id]);
                writer.WriteQuaternion(Rotations[id]);
                writer.WriteVector3(Velocities[id]);
                writer.WriteVector3(AngularVelocities[id]);
            }
        }
        
        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            var length = reader.ReadByte(ref context);
            for (int i = 0; i < length; i++)
            {
                var id = reader.ReadByte(ref context);
                Positions[id] = reader.ReadVector3(ref context);
                Rotations[id] = reader.ReadQuaternion(ref context);
                Velocities[id] = reader.ReadVector3(ref context);
                AngularVelocities[id] = reader.ReadVector3(ref context);
            }
        }
    }
}