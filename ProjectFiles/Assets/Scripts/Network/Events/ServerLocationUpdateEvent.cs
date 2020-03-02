using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Entity;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events {
    public class ServerLocationUpdateEvent : Event {
        public Dictionary<int, PlayerPosition> PlayerPositions { get; } = new Dictionary<int, PlayerPosition>();
        
        private int count;

        public ServerLocationUpdateEvent() {
            ID     = EventType.ServerLocationUpdate;
            Length = 1;
        }

        public ServerLocationUpdateEvent(World world) : this() {
            Length = sizeof(byte) + sizeof(ushort) +
                     world.GetNumPlayers() * (sizeof(ushort) + (3 + 4 + 3 + 3) * sizeof(float));
            foreach (var pair in world.Players) {
                var id  = pair.Key;
                var playerPosition = pair.Value.GetPosition();
            }
        }

        

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write((ushort) PlayerPositions.Count);
            foreach (var kv in PlayerPositions) {
                writer.Write((ushort) kv.Key);
                writer.WritePlayerPosition(kv.Value);
            }
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            var length = reader.ReadUShort(ref context);
            for (var i = 0; i < length; i++) {
                var id = reader.ReadUShort(ref context); 
                PlayerPositions[id] = reader.ReadPlayerPosition(ref context);
            }

            Length = sizeof(byte) + sizeof(ushort) + length * (sizeof(ushort) + (3 + 4 + 3 + 3) * sizeof(float));
        }

        public void UpdateLocations(World world) {
            foreach (var playerID in PlayerPositions.Keys.Where(k => k != ClientConfig.PlayerID &&
                                                               world.Players.Keys.Contains(k))) {
                world.MovePlayer(playerID, PlayerPositions[playerID]);
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