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
    public class Server
    {
        private bool acceptingNewPlayers;
        private NativeList<int> playersToSpawn;
        
        private UdpCNetworkDriver driver;
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
            playersToSpawn = new NativeList<int>(Allocator.Persistent);
            acceptingNewPlayers = true;
            
            // TODO: can simulate bad network conditions here by changing pipeline params
            // ReliableSequenced might not be the best choice 
            driver = new UdpCNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
            pipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        }

        public bool Start()
        {
            if (driver.Bind(NetworkEndPoint.Parse(IP, Port)) != 0)
            {
                Debug.Log($"Server: Failed to bind to port {IP}:{Port}. Is the port already in use?");
                Shutdown();
                return false;
            }
            driver.Listen();
            Debug.Log($"Server: Listening at port {IP}:{Port}...");
            
            return true;
        }
        
        public void Shutdown()
        {
            driver.Dispose();
            connections.Dispose();
        }

        public void HandleNetworkEvents()
        {
            driver.ScheduleUpdate().Complete();
            // Clean up connections
            for (var i = 0; i < connections.Length; i++)
            {
                if (!connections[i].IsCreated)
                {
                    connections.RemoveAtSwapBack(i);
                    i--;
                }
            }

            // Process new connections
            NetworkConnection c;
            if (acceptingNewPlayers)
            {
                while ((c = driver.Accept()) != default(NetworkConnection))
                {
                    connections.Add(c);
                    var endpoint = driver.RemoteEndPoint(c);
                    Debug.Log($"Server: Accepted a connection from {endpoint.IpAddress()}:{endpoint.Port}.");
                }
            }
            else
            {
                while ((c = driver.Accept()) != default(NetworkConnection))
                {
                    c.Disconnect(driver);
                }
            }

            // Process events since the last update
            for (var i = 0; i < connections.Length; i++)
            {
                var connection = connections[i];
                
                if (!connection.IsCreated)
                {
                    continue;
                }
                
                var endpoint = driver.RemoteEndPoint(connection);
                NetworkEvent.Type command;
                while ((command = driver.PopEventForConnection(connection, out var reader)) != NetworkEvent.Type.Empty)
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
                            var playerID = connections[i].InternalId;
                            // Remove from world and player id mapping
                            world.DestroyPlayer(playerID);
                    
                            // Notify users of disconnect
                            var disconnectEvent = new ServerDisconnectEvent((ushort) playerID);
                            SendToAll(disconnectEvent);
                    
                            // Destroy the actual network connection
                            Debug.Log($"Server: Destroyed player { playerID } due to disconnect.");
                            connections[i] = default(NetworkConnection);
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
            playersToSpawn.Add(playerID);
        }

        public void Handle(ClientLocationUpdateEvent ev, NetworkConnection srcConnection)
        {
            var playerID = srcConnection.InternalId;
            
            world.SetPlayerPosition(playerID, ev.Position);
            world.SetPlayerRotation(playerID, ev.Rotation);
            world.SetPlayerVelocity(playerID, ev.Velocity);
            world.SetPlayerAngularVelocity(playerID, ev.AngularVelocity);
        }

        private void SendToAll(Event ev)
        {
            using (var writer = new DataStreamWriter(ev.Length, Allocator.Temp))
            {
                ev.Serialise(writer);
                foreach (var connection in connections)
                {
                    driver.Send(pipeline, connection, writer);
                }
            }
        }
        
        // Send Messages.
        public void SendLocationUpdates()
        {
            if (world.GetNumPlayers() == 0) return;
            var locationUpdate = new ServerLocationUpdateEvent(world);
            SendToAll(locationUpdate);
        }

        public void OnStartGame(ushort nPlayers)
        {
            acceptingNewPlayers = false;
            foreach (var playerID in playersToSpawn)
            {
                var srcConnection = connections[playerID];

                // Once this is called, do not accept new connections.
                // Create ServerStartGameEvent.
                // throw new NotImplementedException();
                // Get a player id
                world.SpawnPlayer(playerID);

                // Get spawn location
                var transform = world.GetPlayerTransform(playerID);
                var handshakeResponse = new ServerHandshakeEvent(transform, playerID);
                using (var writer = new DataStreamWriter(handshakeResponse.Length, Allocator.Temp))
                {
                    handshakeResponse.Serialise(writer);
                    // respond to the handshake
                    driver.Send(pipeline, srcConnection, writer);
                }

                // tell other clients this client has spawned
                var spawnNewPlayer = new ServerSpawnPlayerEvent(transform, playerID);
                using (var writer = new DataStreamWriter(spawnNewPlayer.Length, Allocator.Temp))
                {
                    spawnNewPlayer.Serialise(writer);
                    foreach (var otherClient in connections)
                    {
                        if (otherClient.InternalId == playerID) continue;
                        driver.Send(pipeline, otherClient, writer);
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
                        driver.Send(pipeline, srcConnection, writer);
                    }
                }
            }
            playersToSpawn.Clear();
        }
        
        public void OnPreRoundStart(ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort nPlayers, List<ushort> spacesActive)
        {
            if (world.GetNumPlayers() == 0) return;
            var preRoundStart = new ServerPreRoundStartEvent(roundNumber, preRoundLength, roundLength, nPlayers, spacesActive);
            SendToAll(preRoundStart);
        }

        public void OnRoundStart(ushort roundNumber)
        {
            if (world.GetNumPlayers() == 0) return;
            var roundStart = new ServerRoundStartEvent(roundNumber);
            SendToAll(roundStart);
        }

        public void OnRoundEnd(ushort roundNumber)
        {
            if (world.GetNumPlayers() == 0) return;
            var roundEnd = new ServerRoundEndEvent(roundNumber);
            SendToAll(roundEnd);
        }

        public void OnEliminatePlayers(ushort roundNumber, List<ushort> players)
        {
            if (world.GetNumPlayers() == 0) return;
            var eliminatePlayers = new ServerEliminatePlayersEvent(roundNumber, players);
            SendToAll(eliminatePlayers);
        }
    }
}
