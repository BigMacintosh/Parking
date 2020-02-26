using System;
using System.Collections.Generic;
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
        public event SpaceEnterDelegate SpaceEnterEvent;
        public event SpaceExitDelegate SpaceExitEvent;
        public event TriggerGameStartDelegate TriggerGameStartEvent;
        
        private bool acceptingNewPlayers;
        private List<int> playersToSpawn;
        private int adminClient = -1;
        
        private UdpCNetworkDriver driver;
        private NativeList<NetworkConnection> connections;
        private NetworkPipeline pipeline;
        private World world;
        private Utils.Timer keepAliveTimer;
        
        public ServerConfig Config { get; private set; }
        private string IP => Config.IpAddress;
        private ushort Port => Config.Port;
        
        public Server(World world, ServerConfig config)
        {
            this.world = world;
            Config = config;
            connections = new NativeList<NetworkConnection>(Config.MaxPlayers, Allocator.Persistent);
            playersToSpawn = new List<int>();
            acceptingNewPlayers = true;
            
            // TODO: can simulate bad network conditions here by changing pipeline params
            // ReliableSequenced might not be the best choice 
            driver = new UdpCNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
            pipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            
            // TODO: Keep alive timer does not actually loop
            keepAliveTimer = new Utils.Timer(10, true);
            keepAliveTimer.Tick += OnKeepAlive;
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

            keepAliveTimer.Start();
            return true;
        }
        
        public void Shutdown()
        {
            foreach (var connection in connections)
            {
                connection.Disconnect(driver);
            }

            driver.Dispose();
            connections.Dispose();
        }

        // Send Messages.
        public void SendLocationUpdates()
        {
            if (world.GetNumPlayers() == 0) return;
            var locationUpdate = new ServerLocationUpdateEvent(world);
            sendToAll(locationUpdate);
        }
        
        public void HandleNetworkEvents()
        {
            // Players are in a waiting state so no packets being sent, therefore keep updating the keepAliveTimer.
            if (acceptingNewPlayers) keepAliveTimer.Update();
            
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
                            // Remove this player from the world if they are spawned.
                            if (playerID != adminClient)
                            {
                                if (!acceptingNewPlayers) world.DestroyPlayer(playerID);
                                
                                // Notify users of disconnect
                                var disconnectEvent = new ServerDisconnectEvent((ushort) playerID);
                                sendToAll(disconnectEvent);
                            }
                            
                            // Remove the admin client
                            if (playerID == adminClient) adminClient = -1;

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
                case EventType.ClientSpaceEnterEvent:
                {
                    ev = new ClientSpaceEnterEvent();
                    break;
                }
                case EventType.ClientSpaceExitEvent:
                {
                    ev = new ClientSpaceExitEvent();
                    break;
                }
                case EventType.AdminClientStartGameEvent:
                {
                    ev = new AdminClientStartGameEvent();
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
        
        
        // Handle Event methods
        public void Handle(Event ev, NetworkConnection srcConnection) {
            throw new ArgumentException("Server received an event that it cannot handle");
        }

        public void Handle(ClientHandshakeEvent ev, NetworkConnection srcConnection)
        {
            var playerID = srcConnection.InternalId;
            if (ev.GameMode == GameMode.PlayerMode)
            {
                Debug.Log($"Server: Received handshake from { playerID }.");
                playersToSpawn.Add(playerID);
                var handshakeResponse = new ServerHandshakeEvent(playerID);
                using (var writer = new DataStreamWriter(handshakeResponse.Length, Allocator.Temp))
                {
                    handshakeResponse.Serialise(writer);
                    driver.Send(pipeline, srcConnection, writer);
                }
            } 
            else if (ev.GameMode == GameMode.AdminMode)
            {
                Debug.Log($"Server: Admin Client attempting to connect with id {playerID}");
                if (adminClient == -1)
                {
                    // Accept the new admin
                    Debug.Log($"Server: Accepting new admin user {playerID}");
                    adminClient = playerID;
                    var handshakeResponse = new ServerHandshakeEvent(playerID);
                    using (var writer = new DataStreamWriter(handshakeResponse.Length, Allocator.Temp))
                    {
                        handshakeResponse.Serialise(writer);
                        driver.Send(pipeline, srcConnection, writer);
                    }
                }
                else
                {
                    // Already have an admin client, drop connection.
                    Debug.Log($"Server: Rejecting admin connection, already admin");
                    srcConnection.Disconnect(driver);
                }
                
            }
            
            
        }

        public void Handle(ClientLocationUpdateEvent ev, NetworkConnection srcConnection)
        {
            ev.UpdateLocation(world, srcConnection.InternalId);
        }

        public void Handle(ClientSpaceEnterEvent ev, NetworkConnection srcConnection)
        {
            SpaceEnterEvent?.Invoke(srcConnection.InternalId, ev.SpaceID);
        }

        public void Handle(ClientSpaceExitEvent ev, NetworkConnection srcConnection)
        {
            SpaceExitEvent?.Invoke(srcConnection.InternalId, ev.SpaceID);
        }

        public void Handle(AdminClientStartGameEvent ev, NetworkConnection srcConnection)
        {
            if (srcConnection.InternalId == adminClient)
            {
                TriggerGameStartEvent?.Invoke();
            }
        }
        
        // Used to send a packet to all clients.
        private void sendToAll(Event ev)
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

        private void sendToClient(NetworkConnection connection, Event ev)
        {
            using (var writer = new DataStreamWriter(ev.Length, Allocator.Temp))
            {
                ev.Serialise(writer);
                driver.Send(pipeline, connection, writer);
            }
        }

        public void OnStartGame(ushort freeRoamLength, ushort nPlayers)
        {
            // Now game has started we do not need to keep connections alive.
            keepAliveTimer.Stop();
            
            // Once game is started, do not accept new connections.
            acceptingNewPlayers = false;

            // Spawn all the players in the server world
            world.SpawnPlayers(playersToSpawn);
            
            var spawnPlayersEvent = new ServerGameStart(world, freeRoamLength);
            sendToAll(spawnPlayersEvent);
            playersToSpawn.Clear();
        }
 
        

        public void OnKeepAlive(int ticksLeft)
        {
            if (world.GetNumPlayers() == 0) return;
            var keepAlive = new ServerKeepAlive();
            sendToAll(keepAlive);
        }

        public void OnPreRoundStart(ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort nPlayers)
        {
            if (world.GetNumPlayers() == 0) return;
            var preRoundStart = new ServerPreRoundStartEvent(roundNumber, preRoundLength, roundLength, nPlayers);
            sendToAll(preRoundStart);
        }

        public void OnRoundStart(ushort roundNumber, List<ushort> spacesActive)
        {
            if (world.GetNumPlayers() == 0) return;
            var roundStart = new ServerRoundStartEvent(roundNumber, spacesActive);
            sendToAll(roundStart);
        }

        public void OnRoundEnd(ushort roundNumber)
        {
            if (world.GetNumPlayers() == 0) return;
            var roundEnd = new ServerRoundEndEvent(roundNumber);
            sendToAll(roundEnd);
        }

        public void OnEliminatePlayers(ushort roundNumber, List<int> players)
        {
            if (world.GetNumPlayers() == 0) return;
            var eliminatePlayers = new ServerEliminatePlayersEvent(roundNumber, players);
            sendToAll(eliminatePlayers);
            foreach (var player in players)
            {
                connections[player].Disconnect(driver);
            }
        }

        public void OnGameEnd()
        {
            if (world.GetNumPlayers() == 0) return;
            var gameEnd = new ServerGameEndEvent();
            sendToAll(gameEnd);
        }

        public void OnSpaceClaimed(int playerID, ushort spaceID)
        {
            if (world.GetNumPlayers() == 0) return;
            var claimSpace = new ServerSpaceClaimedEvent(playerID, spaceID);
            sendToAll(claimSpace);
        }
    }
}
