using System.Collections.Generic;
using Game;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events
{
    public class ServerSpawnPlayerEvent : Event
    {
        public Vector3 position;
        public Quaternion rotation;
        
        public ServerSpawnPlayerEvent() {}

        public ServerSpawnPlayerEvent(World world, int playerID)
        {
            ID = 0x03;
            Length = sizeof(float) * (3 + 4) + 1;
            Transform transform = world.GetPlayerTransform(playerID);
            position = transform.position;
            rotation = transform.rotation;
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            position.Serialise(writer);
            rotation.Serialise(writer);
        }

        public override void Deserialise(DataStreamReader reader, DataStreamReader.Context context)
        {
            var id = reader.ReadByte(ref context);
            position = new Vector3();
            position.Deserialise(reader, context);
            rotation = new Quaternion();
            rotation.Deserialise(reader, context);
        }
    }
}