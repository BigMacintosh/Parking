using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events {
    public class ServerGameEndEvent : Event {
        public ServerGameEndEvent() {
            ID = EventType.ServerGameEndEvent;
        }

        public ServerGameEndEvent(List<int> winners) : this() {
            ID      = EventType.ServerGameEndEvent;
            Winners = winners;
            Length  = sizeof(ushort) + Winners.Count * sizeof(int) + sizeof(byte);
        }

        public List<int> Winners { get; private set; }

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            Debug.Log("test1");
            writer.Write((ushort) Winners.Count);
            foreach (var p in Winners) {
                Debug.Log($"Writer: {p}");
                writer.Write(p);
            }
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            var playerCount = reader.ReadUShort(ref context);
            Winners = new List<int>();
            for (var i = 0; i < playerCount; i++) {
                Winners.Add(reader.ReadInt(ref context));
            }

            Length = sizeof(ushort) + Winners.Count * sizeof(int) + sizeof(byte);
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}