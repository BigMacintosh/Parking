using System;
using System.Collections.Generic;
using Game;
using Game.Entity;
using Network.Events;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using Event = Network.Events.Event;
using Timer = Utils.Timer;
using UdpCNetworkDriver =
    Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket,
        Unity.Networking.Transport.DefaultPipelineStageCollection>;

namespace Network {
    public class Server {
        // Delegates
        public event SpaceEnterDelegate       SpaceEnterEvent;
        public event SpaceExitDelegate        SpaceExitEvent;
        public event TriggerGameStartDelegate TriggerGameStartEvent;

        // Public Fields


        // Private Fields
        private bool acceptingNewPlayers;
        private int  adminClient = -1;

        private string IP   => config.IpAddress;
        private ushort Port => config.Port;

        private NativeList<NetworkConnection> connections;
        private UdpCNetworkDriver             driver;

        private readonly Timer           keepAliveTimer;
        private readonly NetworkPipeline pipeline;
        private readonly List<int>       playersToSpawn;
        private readonly World           world;
        private readonly ServerConfig    config;

        private readonly Dictionary<int, Queue<Event>> connectionEventQueues;


        public Server(World world, ServerConfig config) {
            this.world            = world;
            this.config           = config;
            connections           = new NativeList<NetworkConnection>(this.config.MaxPlayers, Allocator.Persistent);
            playersToSpawn        = new List<int>();
            acceptingNewPlayers   = true;
            connectionEventQueues = new Dictionary<int, Queue<Event>>();

            // TODO: can simulate bad network conditions here by changing pipeline params
            // ReliableSequenced might not be the best choice 
            driver   = new UdpCNetworkDriver(new ReliableUtility.Parameters {WindowSize = 32});
            pipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

            keepAliveTimer      =  new Timer(10, true);
            keepAliveTimer.Tick += OnKeepAlive;
        }


        public bool Start() {
            if (driver.Bind(NetworkEndPoint.Parse(IP, Port)) != 0) {
                Debug.Log($"Server: Failed to bind to port {IP}:{Port}. Is the port already in use?");
                Shutdown();
                return false;
            }

            driver.Listen();
            Debug.Log($"Server: Listening at port {IP}:{Port}...");

            keepAliveTimer.Start();
            return true;
        }

        public void Shutdown() {
            foreach (var connection in connections) {
                connection.Disconnect(driver);
            }

            driver.Dispose();
            connections.Dispose();
        }

        // Send Messages.
        public void SendEvents() {
            if (world.GetNumPlayers() == 0) return;

            // generate location updates
            var locationUpdate = new ServerLocationUpdateEvent(world);
            SendToAll(locationUpdate);

            // send all events in queue
            foreach (var connection in connections) {
                var queue = connectionEventQueues[connection.InternalId];

                if (queue.Count == 0) {
                    Debug.Log($"Queue for { connection.InternalId } empty.");
                }
                
                while (queue.Count > 0) {
                    var ev = queue.Dequeue();
                    
                    Debug.Log($"{ ev } dequeued for client { connection.InternalId }.");
                    
                    // driver.Send(pipeline, connection, )
                }
            }
        }

        public void HandleNetworkEvents() {
            // Players are in a waiting state so no packets being sent, therefore keep updating the keepAliveTimer.
            if (acceptingNewPlayers) keepAliveTimer.Update();

            driver.ScheduleUpdate().Complete();
            // Clean up connections
            for (var i = 0; i < connections.Length; i++) {
                if (!connections[i].IsCreated) {
                    connections.RemoveAtSwapBack(i);
                    i--;
                }
            }

            // Process new connections
            NetworkConnection c;
            if (acceptingNewPlayers) {
                while ((c = driver.Accept()) != default) {
                    connections.Add(c);
                    var endpoint = driver.RemoteEndPoint(c);
                    Debug.Log($"Server: Accepted a connection from {endpoint.IpAddress()}:{endpoint.Port}.");
                    
                    connectionEventQueues.Add(c.InternalId, new Queue<Event>());
                }
            } else {
                while ((c = driver.Accept()) != default) {
                    c.Disconnect(driver);
                }
            }

            // Process events since the last update
            for (var i = 0; i < connections.Length; i++) {
                var connection = connections[i];

                if (!connection.IsCreated) {
                    continue;
                }

                var               endpoint = driver.RemoteEndPoint(connection);
                NetworkEvent.Type command;
                while ((command = driver.PopEventForConnection(connection, out var reader)) !=
                       NetworkEvent.Type.Empty) {
                    switch (command) {
                        case NetworkEvent.Type.Data: {
                            var readerContext = default(DataStreamReader.Context);
                            var ev            = (EventType) reader.ReadByte(ref readerContext);
                            HandleEvent(connections[i], endpoint, ev, reader, readerContext);
                            break;
                        }
                        case NetworkEvent.Type.Disconnect:
                            var playerID = connections[i].InternalId;
                            // Remove this player from the world if they are spawned.
                            if (playerID != adminClient) {
                                if (!acceptingNewPlayers) world.DestroyPlayer(playerID);

                                // Notify users of disconnect
                                var disconnectEvent = new ServerDisconnectEvent((ushort) playerID);
                                SendToAll(disconnectEvent);
                            }

                            // Remove the admin client
                            if (playerID == adminClient) adminClient = -1;

                            // Delete this player's event queue
                            connectionEventQueues.Remove(playerID);
                            
                            // Destroy the actual network connection
                            Debug.Log($"Server: Destroyed player {playerID} due to disconnect.");
                            connections[i] = default;
                            break;
                    }
                }
            }
        }

        private void HandleEvent(NetworkConnection connection, NetworkEndPoint          endpoint, EventType eventType,
                                 DataStreamReader  reader,     DataStreamReader.Context readerContext) {
            Event ev;

            switch (eventType) {
                case EventType.ClientHandshake: {
                    ev = new ClientHandshakeEvent();
                    break;
                }
                case EventType.ClientLocationUpdate: {
                    ev = new ClientLocationUpdateEvent();
                    break;
                }
                case EventType.ClientSpaceEnterEvent: {
                    ev = new ClientSpaceEnterEvent();
                    break;
                }
                case EventType.ClientSpaceExitEvent: {
                    ev = new ClientSpaceExitEvent();
                    break;
                }
                case EventType.AdminClientStartGameEvent: {
                    ev = new AdminClientStartGameEvent();
                    break;
                }
                default: {
                    Debug.Log($"Server: Received unexpected event {eventType}.");
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

        public void Handle(ClientHandshakeEvent ev, NetworkConnection srcConnection) {
            var playerID = srcConnection.InternalId;
            switch (ev.GameMode) {
                case GameMode.PlayerMode: {
                    Debug.Log($"Server: Received handshake from {playerID}.");
                    playersToSpawn.Add(playerID);
                    
                    var handshakeResponse = new ServerHandshakeEvent(playerID);
                    SendToClient(srcConnection, handshakeResponse);
                    
                    break;
                }
                case GameMode.AdminMode: {
                    Debug.Log($"Server: Admin Client attempting to connect with id {playerID}");
                    if (adminClient == -1) {
                        // Accept the new admin
                        Debug.Log($"Server: Accepting new admin user {playerID}");
                        adminClient = playerID;
                        var handshakeResponse = new ServerHandshakeEvent(playerID);
                        SendToClient(srcConnection, handshakeResponse);
                    } else {
                        // Already have an admin client, drop connection.
                        Debug.Log("Server: Rejecting admin connection, already admin");
                        srcConnection.Disconnect(driver);
                    }

                    break;
                }
                default:
                    Debug.Log("Unknown GameMode");
                    break;
            }
        }

        public void Handle(ClientLocationUpdateEvent ev, NetworkConnection srcConnection) {
            ev.UpdateLocation(world, srcConnection.InternalId);
        }

        public void Handle(ClientSpaceEnterEvent ev, NetworkConnection srcConnection) {
            SpaceEnterEvent?.Invoke(srcConnection.InternalId, ev.SpaceID);
        }

        public void Handle(ClientSpaceExitEvent ev, NetworkConnection srcConnection) {
            SpaceExitEvent?.Invoke(srcConnection.InternalId, ev.SpaceID);
        }

        public void Handle(AdminClientStartGameEvent ev, NetworkConnection srcConnection) {
            if (srcConnection.InternalId == adminClient) {
                TriggerGameStartEvent?.Invoke();
            }
        }

        // Used to send a packet to all clients.
        private void SendToAll(Event ev) {
            foreach (var connection in connections) {
                connectionEventQueues[connection.InternalId].Enqueue(ev);
            }
        }

        private void SendToClient(NetworkConnection connection, Event ev) {
            Debug.Log($"Queueing event { ev } for client { connection.InternalId }.");
            connectionEventQueues[connection.InternalId].Enqueue(ev);
        }

        public void OnStartGame(ushort freeRoamLength, ushort nPlayers) {
            // Now game has started we do not need to keep connections alive.
            keepAliveTimer.Stop();

            // Once game is started, do not accept new connections.
            acceptingNewPlayers = false;

            // Spawn all the players in the server world
            world.SpawnPlayers(playersToSpawn);

            var spawnPlayersEvent = new ServerGameStart(world, freeRoamLength);
            SendToAll(spawnPlayersEvent);
            playersToSpawn.Clear();
        }


        private void OnKeepAlive(int ticksLeft) {
            var keepAlive = new ServerKeepAlive();
            SendToAll(keepAlive);
        }

        public void OnPreRoundStart(ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort nPlayers) {
            if (world.GetNumPlayers() == 0) return;
            var preRoundStart = new ServerPreRoundStartEvent(roundNumber, preRoundLength, roundLength, nPlayers);
            SendToAll(preRoundStart);
        }

        public void OnRoundStart(ushort roundNumber, List<ushort> spacesActive) {
            if (world.GetNumPlayers() == 0) return;
            var roundStart = new ServerRoundStartEvent(roundNumber, spacesActive);
            SendToAll(roundStart);
        }

        public void OnRoundEnd(ushort roundNumber) {
            if (world.GetNumPlayers() == 0) return;
            var roundEnd = new ServerRoundEndEvent(roundNumber);
            SendToAll(roundEnd);
        }

        public void OnEliminatePlayers(ushort roundNumber, List<int> players) {
            if (world.GetNumPlayers() == 0) return;
            var eliminatePlayers = new ServerEliminatePlayersEvent(roundNumber, players);
            SendToAll(eliminatePlayers);
            foreach (var player in players) {
                // TODO: Chris no likey
                int index;
                for (index = 0; index != connections[index].InternalId && index < connections.Length; index++) { }

                connections[index].Disconnect(driver);
                world.DestroyPlayer(player);

                // Notify users of disconnect
                var disconnectEvent = new ServerDisconnectEvent((ushort) player);
                SendToAll(disconnectEvent);

                Debug.Log($"Server: Destroyed player {player} due to disconnect.");
                connections[index] = default;
            }
        }

        public void OnGameEnd(List<int> winners) {
            var gameEnd = new ServerGameEndEvent(winners);
            SendToAll(gameEnd);
        }

        public void OnSpaceClaimed(int playerID, ushort spaceID) {
            if (world.GetNumPlayers() == 0) return;
            var claimSpace = new ServerSpaceClaimedEvent(playerID, spaceID);
            SendToAll(claimSpace);
        }
    }
}