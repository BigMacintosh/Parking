using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Driving;
using Game.Entity;
using Unity.Networking.Transport;

namespace Network.Events {
    public class ServerLocationUpdateEvent : Event {
        public ulong                           Tick            { get; private set; }
        public Dictionary<int, ValueTuple<PlayerPosition, VehicleInputState>> PlayerPositions { get; } = new Dictionary<int, ValueTuple<PlayerPosition, VehicleInputState>>();

        private int count;

        public ServerLocationUpdateEvent() {
            ID     = EventType.ServerLocationUpdate;
            Length = 1;
        }

        public ServerLocationUpdateEvent(ulong tick, Dictionary<int, VehicleInputState> inputs, World world) : this() {
            Tick = tick;
            foreach (var pair in world.Players) {
                var id             = pair.Key;
                var playerPosition = pair.Value.GetPosition();
                var playerInput    = inputs[id];
                PlayerPositions.Add(id, (playerPosition, playerInput));
            }
            // hi samie 
            Length = sizeof(byte) + sizeof(ulong) + sizeof(ushort) +
                     PlayerPositions.Sum(kv => sizeof(int) + kv.Value.Item1.WriterLength() + kv.Value.Item2.WriterLength());
        }


        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(Tick);
            writer.Write((ushort) PlayerPositions.Count);
            foreach (var kv in PlayerPositions) {
                writer.Write(kv.Key);
                writer.WritePlayerPosition(kv.Value.Item1);
                writer.WriteVehicleInputState(kv.Value.Item2);
            }
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            Tick = reader.ReadULong(ref context);
            var length = reader.ReadUShort(ref context);
            for (var i = 0; i < length; i++) {
                var id = reader.ReadInt(ref context);

                var pos = reader.ReadPlayerPosition(ref context);
                var inputs = reader.ReadVehicleInputState(ref context);
                PlayerPositions[id] = (pos, inputs);
            }

            Length = sizeof(byte) + sizeof(ulong) + sizeof(ushort) +
                     PlayerPositions.Sum(kv => sizeof(int) + kv.Value.Item1.WriterLength() + kv.Value.Item2.WriterLength());
        }

        public void UpdateLocations(World world) {
            foreach (var playerID in PlayerPositions.Keys) {
                world.MovePlayer(playerID, PlayerPositions[playerID].Item1);
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