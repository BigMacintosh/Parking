using System.Collections.Generic;
using Game;
using Game.Entity;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events {
    public class ServerGameStart : Event {
        public ushort                          FreeRoamLength  { get; private set; }
        public Dictionary<int, PlayerPosition> PlayerPositions { get; } = new Dictionary<int, PlayerPosition>();

        private int count;


        public ServerGameStart() {
            ID = EventType.ServerGameStartEvent;
        }

        public ServerGameStart(World world, ushort freeRoamLength) : this() {
            FreeRoamLength = freeRoamLength;
            foreach (var pair in world.Players) {
                var id = pair.Key;
                PlayerPositions[id] = pair.Value.GetPosition();
            }

            Length = sizeof(byte) + sizeof(ushort) + sizeof(ushort) +
                     world.GetNumPlayers() * (sizeof(ushort) + (3 + 4) * sizeof(float));
        }


        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(FreeRoamLength);
            writer.Write((ushort) PlayerPositions.Count);
            foreach (var id in PlayerPositions.Keys) {
                writer.Write((ushort) id);
                writer.WritePlayerPosition(PlayerPositions[id]);
            }
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            FreeRoamLength = reader.ReadUShort(ref context);
            var length = reader.ReadUShort(ref context);
            for (var i = 0; i < length; i++) {
                var id = reader.ReadUShort(ref context);
                PlayerPositions[id] = reader.ReadPlayerPosition(ref context);
            }

            Length = sizeof(byte) + sizeof(ushort) + length * (sizeof(ushort) + (3 + 4) * sizeof(float));
        }

        public void SpawnPlayers(ClientWorld world) {
            world.SpawnPlayers(PlayerPositions);
            
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}