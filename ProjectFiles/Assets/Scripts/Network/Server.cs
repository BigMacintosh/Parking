using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Network.Events;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using Event = Network.Events.Event;
using NetworkConnection = Unity.Networking.Transport.NetworkConnection; 
using UdpCNetworkDriver = Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket,Unity.Networking.Transport.DefaultPipelineStageCollection>;

namespace Network
{
    public class Server : IRoundObserver
    {
        private UdpCNetworkDriver Driver;
        private NativeList<NetworkConnection> connections;
        private NetworkPipeline pipeline;
        private World world;
        
        public ServerConfig Config { get; private set; }
        private string IP => Config.IpAddress;
        private ushort Port => Config.Port;
        
        public Server(World world, ServerConfig config)
        {
            this.world = world;
            Config = config;
            connections = new NativeList<NetworkConnection>(Config.MaxPlayers, Allocator.Persistent);
            
            // TODO: can simulate bad network conditions here by changing pipeline params
            // ReliableSequenced might not be the best choice 
            Driver = new UdpCNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
            pipeline = Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        }

        public bool Start()
        {
            if (Driver.Bind(NetworkEndPoint.Parse(IP, Port)) != 0)
            {
                Debug.Log($"Server: Failed to bind to port {IP}:{Port}. Is the port already in use?");
                Shutdown();
                return false;
            }
            Driver.Listen();
            Debug.Log($"Server: Listening at port {IP}:{Port}...");
            
            return true;
        }
        
        public void Shutdown()
        {
            Driver.Dispose();
            connections.Dispose();
        }

        public void SendLocationUpdates()
        {
            if (world.GetNumPlayers() == 0) return;
            var locationUpdate = new ServerLocationUpdateEvent(world);
            using (var writer = new DataStreamWriter(locationUpdate.Length, Allocator.Temp))
            {
                locationUpdate.Serialise(writer);
                foreach (var connection in connections)
                {
                    Driver.Send(pipeline, connection, writer);
                }
            }
        }

        public void HandleNetworkEvents()
        {
            Driver.ScheduleUpdate().Complete();
            // Clean up connections
            for (var i = 0; i < connections.Length; i++)
            {
                if (!connections[i].IsCreated)
                {
                    var playerID = connections[i].InternalId;
                    
                    // Remove from world and player id mapping
                    world.DestroyPlayer(playerID);

                    // Destroy the actual network connection
                    connections[i] = default(NetworkConnection);
                            
                    Debug.Log($"Server: Destroyed player { playerID } due to disconnect.");
                    
                    connections.RemoveAtSwapBack(i);
                    i--;
                }
            }

            // Process new connections
            NetworkConnection c;
            while ((c = Driver.Accept()) != default(NetworkConnection))
            {
                connections.Add(c);
                var endpoint = Driver.RemoteEndPoint(c);
                Debug.Log($"Server: Accepted a connection from {endpoint.IpAddress()}:{endpoint.Port}.");
            }
        
            // Process events since the last update
            for (var i = 0; i < connections.Length; i++)
            {
                var connection = connections[i];
                
                if (!connection.IsCreated)
                {
                    continue;
                }
                
                var endpoint = Driver.RemoteEndPoint(connection);
                NetworkEvent.Type command;
                while ((command = Driver.PopEventForConnection(connection, out var reader)) != NetworkEvent.Type.Empty)
                {
                    switch (command)
                    {
                        case NetworkEvent.Type.Data:
                        {
                            var readerContext = default(DataStreamReader.Context);
                            var ev = (EventType) reader.ReadByte(ref readerContext);
                            HandleEvent(connections[i], endpoint, ev, reader, readerContext);
                            break;
                        }
                        case NetworkEvent.Type.Disconnect:
                            Debug.Log($"Server: {endpoint.IpAddress()}:{endpoint.Port} disconnected.");

                            break;
                    }
                }
            }
        }

        private void HandleEvent(NetworkConnection connection, NetworkEndPoint endpoint, EventType eventType, 
                                 DataStreamReader reader, DataStreamReader.Context readerContext)
        {
            Event ev;

            switch (eventType)
            {
                case EventType.ClientHandshake:
                {
                    ev = new ClientHandshakeEvent();
                    break;
                }
                case EventType.ClientLocationUpdate:
                {
                    ev = new ClientLocationUpdateEvent();
                    break;
                }
                default:
                {
                    Debug.Log($"Server: Received unexpected event { eventType }.");
                    return;
                }
            }
            ev.Deserialise(reader, ref readerContext);
            ev.Handle(this, connection);
        }
        
        public void Handle(Event ev, NetworkConnection srcConnection) {
            throw new ArgumentException("Server received an event that it cannot handle");
        }

        public void Handle(ClientHandshakeEvent ev, NetworkConnection srcConnection)
        {
            var playerID = srcConnection.InternalId;
            Debug.Log($"Server: Received handshake from { playerID }.");
            
            // Get a player id
            world.SpawnPlayer(playerID);

            // Get spawn location
            var transform = world.GetPlayerTransform(playerID);
            var handshakeResponse = new ServerHandshakeEvent(transform, playerID);
            using (var writer = new DataStreamWriter(handshakeResponse.Length, Allocator.Temp))
            {
                handshakeResponse.Serialise(writer);
                // respond to the handshake
                Driver.Send(pipeline, srcConnection, writer);
            }

            // tell other clients this client has spawned
            var spawnNewPlayer = new ServerSpawnPlayerEvent(transform, playerID);
            using (var writer = new DataStreamWriter(spawnNewPlayer.Length, Allocator.Temp))
            {
                spawnNewPlayer.Serialise(writer);
                foreach (var otherClient in connections)
                {
                    if (otherClient.InternalId == playerID) continue;
                    Driver.Send(pipeline, otherClient, writer);
                }
            }

            // tell new player about the existing players
            foreach (var otherPlayer in world.PlayerIDs.Where(otherPlayer => otherPlayer != playerID))
            {
                var otherTransform = world.GetPlayerTransform(otherPlayer);
                var spawnOtherPlayer = new ServerSpawnPlayerEvent(otherTransform, otherPlayer);
                using (var writer = new DataStreamWriter(spawnOtherPlayer.Length, Allocator.Temp))
                {
                    spawnOtherPlayer.Serialise(writer);
                    Driver.Send(pipeline, srcConnection, writer);
                }
            }
        }

        public void Handle(ClientLocationUpdateEvent ev, NetworkConnection srcConnection)
        {
            var playerID = srcConnection.InternalId;
            
            world.SetPlayerPosition(playerID, ev.Position);
            world.SetPlayerRotation(playerID, ev.Rotation);
            world.SetPlayerVelocity(playerID, ev.Velocity);
            world.SetPlayerAngularVelocity(playerID, ev.AngularVelocity);
        }

        public void OnPreRoundStart(int roundNumber, int preRoundLength, int roundLength, int nPlayers, List<byte> spacesActive)
        {
            // Pls deal with me :'(
            throw new System.NotImplementedException();
        }

        public void OnRoundStart(int roundNumber)
        {
            // Pls deal with me :'(
            throw new System.NotImplementedException();
        }

        public void OnRoundEnd(int roundNumber)
        {
            // Pls deal with me :'(
            throw new System.NotImplementedException();
        }

        public void OnEliminatePlayers(int roundNumber, List<int> eliminatedPlayers)
        {
            // Pls deal with me :'(
            throw new System.NotImplementedException();
        }
    }
}
