using System.Collections.Generic;
using Game;
using Game.Entity;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events {
    public class ServerGameStart : Event {
        private int count;


        public ServerGameStart() {
            ID = EventType.ServerGameStartEvent;
        }

        public ServerGameStart(World world, ushort freeRoamLength) : this() {
            FreeRoamLength = freeRoamLength;
            foreach (var pair in world.Players) {
                var id  = pair.Key;
                var car = pair.Value;

                var transform = car.transform;
                Positions[id] = transform.position;
                Rotations[id] = transform.rotation;
            }

            Length = sizeof(byte) + sizeof(ushort) + sizeof(ushort) +
                     world.GetNumPlayers() * (sizeof(ushort) + (3 + 4) * sizeof(float));
        }

        public ushort                      FreeRoamLength { get; private set; }
        public Dictionary<int, Vector3>    Positions      { get; } = new Dictionary<int, Vector3>();
        public Dictionary<int, Quaternion> Rotations      { get; } = new Dictionary<int, Quaternion>();

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(FreeRoamLength);
            writer.Write((ushort) Positions.Count);
            foreach (var id in Positions.Keys) {
                writer.Write((ushort) id);
                writer.WriteVector3(Positions[id]);
                writer.WriteQuaternion(Rotations[id]);
            }
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            FreeRoamLength = reader.ReadUShort(ref context);
            var length = reader.ReadUShort(ref context);
            for (var i = 0; i < length; i++) {
                var id = reader.ReadUShort(ref context);
                Positions[id] = reader.ReadVector3(ref context);
                Rotations[id] = reader.ReadQuaternion(ref context);
            }

            Length = sizeof(byte) + sizeof(ushort) + length * (sizeof(ushort) + (3 + 4) * sizeof(float));
        }

        public void SpawnPlayers(World world) {
            foreach (var id in Positions.Keys) {
                world.SpawnPlayer(id, Positions[id], Rotations[id]);
            }
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}