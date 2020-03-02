using Game;
using Game.Entity;
using Unity.Networking.Transport;
using UnityEngine;

namespace Network.Events {
    public class ClientLocationUpdateEvent : Event {
        
        public PlayerPosition PlayerPosition { get; set; }
        public ClientLocationUpdateEvent() {
            ID     = EventType.ClientLocationUpdate;
            Length = (3 + 3 + 3 + 4) * sizeof(float) + sizeof(byte);
        }

        public ClientLocationUpdateEvent(ClientWorld world) : this() {
            PlayerPosition = world.GetMyPosition();
        }

        

        public override void Serialise(DataStreamWriter writer) {
            base.Serialise(writer);
            writer.WritePlayerPosition(PlayerPosition);
        }

        public override void Deserialise(DataStreamReader reader, ref DataStreamReader.Context context) {
            PlayerPosition = reader.ReadPlayerPosition(ref context);
        }

        public void UpdateLocation(World world, int playerID) {
            world.MovePlayer(playerID, PlayerPosition);
        }


        public override void Handle(Server server, NetworkConnection connection) {
            server.Handle(this, connection);
        }

        public override void Handle(Client client, NetworkConnection connection) {
            client.Handle(this, connection);
        }
    }
}