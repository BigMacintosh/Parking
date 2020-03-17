using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entity;
using Network.Events;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using Utils;
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

        private       ulong tick;
        private const ulong TickRate      = 50;
        private const ulong SnapshotRate  = 25;
        private const ulong SnapshotRatio = TickRate / SnapshotRate;
        
        private bool acceptingNewPlayers;
        private int  adminClient = -1;

        private string IP   => config.IpAddress;
        private ushort Port => config.Port;

        private NativeList<NetworkConnection> connections;
        private UdpCNetworkDriver             driver;

        private readonly Timer           keepAliveTimer;
        private readonly NetworkPipeline pipeline;
        private readonly List<int>       playersToSpawn;
        private readonly ServerWorld     world;
        private readonly ServerConfig    config;

        private readonly Dictionary<int, Queue<Event>> connectionEventQueues;
        private readonly Dictionary<int, ulong> lastConfirmedTicks;

        // private HistoryBuffer<GameSnapshot> snapshotHistory;

        public Server(ServerWorld world, ServerConfig config) {
            // snapshotHistory = new HistoryBuffer<GameSnapshot>((int) SnapshotRate);
            lastConfirmedTicks = new Dictionary<int, ulong>();
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
        public void FixedUpdate() {
            tick++;
            if (!acceptingNewPlayers) {
                // generate location updates

                if (tick % SnapshotRatio == 0) {
                    // var snapshot = GameSnapshot.NewGameSnapshot();
                    // snapshot.playerPositions = world.Players.ToDictionary(x => (ushort)x.Key, x => x.Value.GetPosition());
                    // snapshotHistory.Put(snapshot);

                    foreach (var connection in connections) {
                        var lastTick = lastConfirmedTicks[connection.InternalId];
                        var locations = new ServerLocationUpdateEvent(lastTick, world);
                        SendToClient(connection, locations);
                    }
                }
            }

            // send all events in queue
            foreach (var connection in connections) {
                var queue = connectionEventQueues[connection.InternalId];

                var totalLength = queue.Sum(ev => ev.Length);
                using (var writer = new DataStreamWriter(totalLength, Allocator.Temp)) {
                    while (queue.Count > 0) {
                        var ev = queue.Dequeue();

                        ev.Serialise(writer);
                    }

                    driver.Send(pipeline, connection, writer);
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

                var endpoint = driver.RemoteEndPoint(connection);

                NetworkEvent.Type command;
                while ((command = driver.PopEventForConnection(connection, out var reader)) !=
                       NetworkEvent.Type.Empty) {
                    switch (command) {
                        case NetworkEvent.Type.Data: {
                            var readerContext = default(DataStreamReader.Context);

                            while (reader.Length - reader.GetBytesRead(ref readerContext) > 0) {
                                var ev = (EventType) reader.ReadByte(ref readerContext);
                                HandleEvent(connections[i], endpoint, ev, reader, ref readerContext);
                            }

                            break;
                        }
                        case NetworkEvent.Type.Disconnect: {
                            var playerID = connections[i].InternalId;
                            // Remove this player from the world if they are spawned.
                            if (playerID != adminClient) {
                                world.DestroyPlayer(playerID);

                                // Notify users of disconnect
                                var disconnectEvent = new ServerDisconnectEvent((ushort) playerID);
                                SendToAll(disconnectEvent);
                            } else {
                                // Remove the admin client
                                adminClient = -1;
                            }

                            // Delete this player's event queue
                            connectionEventQueues.Remove(playerID);

                            // Destroy the actual network connection
                            Debug.Log($"Server: Destroyed player {playerID} due to disconnect (by the player).");
                            connections[i] = default;
                            break;
                        }
                    }
                }
            }
        }

        private void HandleEvent(NetworkConnection connection, NetworkEndPoint endpoint,
                                 EventType         eventType,
                                 DataStreamReader  reader, ref DataStreamReader.Context readerContext) {
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
                case EventType.ClientInputStateEvent: {
                    ev = new ClientInputStateEvent();
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
            var respond  = false;
            switch (ev.GameMode) {
                case GameMode.PlayerMode: {
                    Debug.Log($"Server: Received handshake from {playerID}.");

                    var handshakeResponse = new ServerHandshakeEvent(
                        tick, 
                        (ulong) DateTimeOffset.Now.ToUnixTimeMilliseconds(), 
                        playerID,
                        world.GetAllPlayerOptions()
                    );
                    SendToClient(srcConnection, handshakeResponse);

                    world.CreatePlayer(playerID, ev.PlayerOptions);

                    var newClient = new ServerNewPlayerConnectedEvent(playerID, ev.PlayerOptions);
                    SendToAll(newClient);

                    break;
                }
                case GameMode.AdminMode: {
                    Debug.Log($"Server: Admin Client attempting to connect with id {playerID}");
                    if (adminClient == -1) {
                        // Accept the new admin
                        Debug.Log($"Server: Accepting new admin user {playerID}");
                        adminClient = playerID;

                        var handshakeResponse = new ServerHandshakeEvent(
                            tick, 
                            (ulong) DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                            playerID,
                            world.GetAllPlayerOptions()
                        );
                        lastConfirmedTicks[srcConnection.InternalId] = tick;
                        SendToClient(srcConnection, handshakeResponse);
                        respond = true;
                    } else {
                        // Already have an admin client, drop connection.
                        Debug.Log("Server: Rejecting admin connection, already admin");
                        srcConnection.Disconnect(driver);
                    }

                    break;
                }
                default: {
                    Debug.Log("Unknown GameMode");
                    break;
                }
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

        public void Handle(ClientInputStateEvent ev, NetworkConnection srcConnection) {
            lastConfirmedTicks[srcConnection.InternalId] = ev.Tick;
            world.ApplyInputs(srcConnection.InternalId, ev.Inputs);
        }

        // Used to send a packet to all clients.
        private void SendToAll(Event ev) {
            foreach (var connection in connections) {
                connectionEventQueues[connection.InternalId].Enqueue(ev);
            }
        }

        private void SendToClient(NetworkConnection connection, Event ev) {
            connectionEventQueues[connection.InternalId].Enqueue(ev);
        }

        public void OnStartGame(ushort freeRoamLength, ushort nPlayers) {
            // Now game has started we do not need to keep connections alive.
            keepAliveTimer.Stop();

            // Once game is started, do not accept new connections.
            acceptingNewPlayers = false;

            // Spawn all the players in the server world
            world.SpawnPlayers();

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

                Debug.Log($"Server: Destroyed player {player} due to disconnect (via elimination).");
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