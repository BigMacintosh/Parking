using System.Collections.Generic;
using Game;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events
{
    public class ServerSpawnPlayerEvent : Event
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        
        public ServerSpawnPlayerEvent() {}

        public ServerSpawnPlayerEvent(World world, int playerID)
        {
            ID = 0x03;
            Length = sizeof(float) * (3 + 4) + 1;
            var transform = world.GetPlayerTransform(playerID);
            Position = transform.position;
            Rotation = transform.rotation;
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.WriteVector3(Position);
            writer.WriteQuaternion(Rotation);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            var id = reader.ReadByte(ref context);
            Position = reader.ReadVector3(ref context);
            Rotation = reader.ReadQuaternion(ref context);
        }
    }
}