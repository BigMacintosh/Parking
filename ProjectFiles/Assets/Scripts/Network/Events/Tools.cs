using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events
{
    public static class Tools
    {
        public static void Serialise(this Vector3 vector, DataStreamWriter writer)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
        }

        public static void Deserialise(this Vector3 vector, DataStreamReader reader, DataStreamReader.Context context)
        {
            vector.x = reader.ReadFloat(ref context);
            vector.y = reader.ReadFloat(ref context);
            vector.z = reader.ReadFloat(ref context);
        }
        
        public static void Serialise(this Quaternion quaternion, DataStreamWriter writer)
        {
            writer.Write(quaternion.x);
            writer.Write(quaternion.y);
            writer.Write(quaternion.z);
            writer.Write(quaternion.w);
        }

        public static void Deserialise(this Quaternion quaternion, DataStreamReader reader, DataStreamReader.Context context)
        {
            quaternion.x = reader.ReadFloat(ref context);
            quaternion.y = reader.ReadFloat(ref context);
            quaternion.z = reader.ReadFloat(ref context);
            quaternion.w = reader.ReadFloat(ref context);
        }
    }
}