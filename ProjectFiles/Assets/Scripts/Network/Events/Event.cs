using Unity.Networking.Transport;

namespace Network.Events
{
    public abstract class Event
    {
        protected byte ID = 0xFF;
        public int Length { get; protected set; }
        
        public virtual void Serialise(DataStreamWriter writer)
        {
            writer.Write(ID);
        }

        public abstract void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context);
    }
}