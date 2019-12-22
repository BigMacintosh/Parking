using Game;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events
{
    public class ServerHandshake : Event
    {
        public int PlayerID { get; private set; }
        public Vector3 Position { get; private set; }
        
        public ServerHandshake() {}

        public ServerHandshake(World world, int playerID)
        {
            ID = 0x01;
            Length = (3 * sizeof(float)) + 1;
            Position = world.GetPlayerTransform(playerID).position;
            PlayerID = playerID;
        }

        public override void Serialise(DataStreamWriter writer)
        {
            base.Serialise(writer);
            writer.Write((byte) PlayerID);
            writer.WriteVector3(Position);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context)
        {
            PlayerID = reader.ReadByte(ref context);
            Position = reader.ReadVector3(ref context);
        }
    }
}