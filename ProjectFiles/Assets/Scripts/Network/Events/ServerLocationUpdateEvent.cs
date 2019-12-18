using System.Collections.Generic;
using Game;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events
{
    public class ServerLocationUpdateEvent : Event
    {
        public Dictionary<int, Vector3> positions { get; } = new Dictionary<int, Vector3>();
        public Dictionary<int, Quaternion> rotations { get; } = new Dictionary<int, Quaternion>();
        public Dictionary<int, Vector3> velocities { get; } = new Dictionary<int, Vector3>();
        public Dictionary<int, Vector3> angularVelocities { get; } = new Dictionary<int, Vector3>();
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
                positions[id] = transform.position;
                rotations[id] = transform.rotation;

                var rb = car.GetComponent<Rigidbody>();
                velocities[id] = rb.velocity;
                angularVelocities[id] = rb.angularVelocity;
            }
        }
        
        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write(Length);
            foreach (var id in positions.Keys)
            {
                positions[id].Serialise(writer);
                rotations[id].Serialise(writer);
                velocities[id].Serialise(writer);
                angularVelocities[id].Serialise(writer);
            }
        }
        
        public override void Deserialise(DataStreamReader reader, DataStreamReader.Context context)
        {
            var length = reader.ReadByte(ref context);
            for (int i = 0; i < length; i++)
            {
                var id = reader.ReadByte(ref context);
                positions[id] = new Vector3();
                positions[id].Deserialise(reader, context);
                rotations[id] = new Quaternion();
                rotations[id].Deserialise(reader, context);
                velocities[id] = new Vector3();
                velocities[id].Deserialise(reader, context);
                angularVelocities[id] = new Vector3();
                angularVelocities[id].Deserialise(reader, context);
            }
        }
    }
}