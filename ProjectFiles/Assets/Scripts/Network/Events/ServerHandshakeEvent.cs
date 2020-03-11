using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entity;
using Unity.Networking.Transport;

namespace Network.Events {
    public class ServerHandshakeEvent : Event {
        public ulong                          BaseTick      { get; private set; }
        public ulong                          Timestamp     { get; private set; }
        public int                            PlayerID      { get; private set; }
        public Dictionary<int, PlayerOptions> PlayerOptions { get; private set; }

        public ServerHandshakeEvent() {
            ID = EventType.ServerHandshake;
        }

        public ServerHandshakeEvent(ulong                          baseTick, ulong timestamp, int playerID,
                                    Dictionary<int, PlayerOptions> playerOptions) :
            this() {
            BaseTick      = baseTick;
            Timestamp     = timestamp;
            PlayerID      = playerID;
            PlayerOptions = playerOptions;

            Length = sizeof(byte) + sizeof(ulong) + sizeof(ulong) + sizeof(int) + sizeof(int) +
                     playerOptions.Sum(k => k.Value.WriterLength() + sizeof(int));
        }


        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.Write(BaseTick);
            writer.Write(Timestamp);
            writer.Write(PlayerID);
            writer.Write(PlayerOptions.Count);
            foreach (var kv in PlayerOptions) {
                writer.Write(kv.Key);
                writer.WritePlayerOptions(kv.Value);
            }
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            BaseTick      = reader.ReadULong(ref context);
            Timestamp     = reader.ReadULong(ref context);
            PlayerID      = reader.ReadInt(ref context);
            PlayerOptions = new Dictionary<int, PlayerOptions>();

            var count = reader.ReadInt(ref context);
            for (var i = 0; i < count; i++) {
                var playerID      = reader.ReadInt(ref context);
                var playerOptions = reader.ReadPlayerOptions(ref context);
                PlayerOptions.Add(playerID, playerOptions);
            }

            Length = sizeof(int) + sizeof(byte) + sizeof(int) +
                     PlayerOptions.Sum(k => k.Value.WriterLength() + sizeof(int));
        }

        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }

        public void Apply(ClientWorld world) {
            world.CreatePlayer(PlayerID, new PlayerOptions {
                CarColour  = ClientConfig.VehicleColour,
                CarType    = ClientConfig.VehicleType,
                PlayerName = ClientConfig.PlayerName,
            }, true);


            // Create all players who have already connected.
            foreach (var kv in PlayerOptions.Where(kv => kv.Key != PlayerID)) {
                world.CreatePlayer(kv.Key, kv.Value);
            }
        }
    }
}