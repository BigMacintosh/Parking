using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Game;
using Game.Core.Driving;
using Game.Entity;
using Network.Events;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using Event = Network.Events.Event;
using UdpCNetworkDriver =
    Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket,
        Unity.Networking.Transport.DefaultPipelineStageCollection>;

namespace Network {
    /// <summary>
    /// An interface that a client needs to implement.
    /// </summary>
    public interface IClient {
        // Delegates
        event GameStartDelegate         GameStartEvent;
        event PreRoundStartDelegate     PreRoundStartEvent;
        event RoundStartDelegate        RoundStartEvent;
        event RoundEndDelegate          RoundEndEvent;
        event SpaceClaimedDelegate      SpaceClaimedEvent;
        event EliminatePlayersDelegate  EliminatePlayersEvent;
        event PlayerCountChangeDelegate PlayerCountChangeEvent;
        event GameEndDelegate           GameEndEvent;

        bool   Start(ushort port = 25565);
        void   Shutdown();
        void   SendEvents(VehicleInputState inputs);
        void   HandleNetworkEvents();
        string GetServerIP();
        void   OnSpaceEnter(int playerID, ushort spaceID);
        void   OnSpaceExit(int  playerID, ushort spaceID);
        void   OnTriggerGameStart();
    }

    public class Client : IClient {
        // Delegates
        public event GameStartDelegate         GameStartEvent;
        public event PreRoundStartDelegate     PreRoundStartEvent;
        public event RoundStartDelegate        RoundStartEvent;
        public event RoundEndDelegate          RoundEndEvent;
        public event EliminatePlayersDelegate  EliminatePlayersEvent;
        public event PlayerCountChangeDelegate PlayerCountChangeEvent;
        public event GameEndDelegate           GameEndEvent;
        public event SpaceClaimedDelegate      SpaceClaimedEvent;

        private ulong tick;
        private const ulong TickDuration = 1000 / 50;
        private const ulong SnapshotDuration = 1000 / 50;

        // Private Fields
        private bool              inGame;
        private bool              done;
        private string            serverIP;
        private ushort            serverPort;
        private UdpCNetworkDriver driver;
        private NetworkConnection connection;

        private readonly NetworkPipeline pipeline;
        private readonly ClientWorld     world;

        private readonly Queue<Event> eventQueue;

        public Client(ClientWorld world) {
            driver     = new UdpCNetworkDriver(new ReliableUtility.Parameters {WindowSize = 32});
            pipeline   = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            done       = false;
            this.world = world;
            eventQueue = new Queue<Event>();
        }

        public static IClient GetDummyClient(ClientWorld world) {
            return new DummyClient(world);
        }

        public bool Start(ushort port = 25565) {
            serverIP   = ClientConfig.ServerIP;
            serverPort = port;

            Debug.Log($"Client: Connecting to {serverIP}:{serverPort}...");

            var endpoint = NetworkEndPoint.Parse(serverIP, serverPort);
            connection = driver.Connect(endpoint);
            return true;
        }

        public void Shutdown() {
            Debug.Log("Client: Shutting down.");
            done = true;
            connection.Close(driver);
            driver.Dispose();
        }

        public void SendEvents(VehicleInputState inputs) {
            tick++;
            // Only send location if in game and in player mode
            if (inGame && ClientConfig.GameMode == GameMode.PlayerMode) {
                var locationUpdate = new ClientInputStateEvent(tick, inputs);
                SendEventToServer(locationUpdate);
            }

            var totalLength = eventQueue.Sum(ev => ev.Length);

            using (var writer = new DataStreamWriter(totalLength, Allocator.Temp)) {
                while (eventQueue.Count > 0) {
                    var ev = eventQueue.Dequeue();
                    
                    ev.Serialise(writer);
                }
                
                driver.Send(pipeline, connection, writer);
            }
        }

        public void HandleNetworkEvents() {
            driver.ScheduleUpdate().Complete();

            // Check connection is valid
            if (!connection.IsCreated) {
                if (!done) {
                    Debug.Log($"Client: Something went wrong when connecting to {serverIP}:{serverPort}.");
                }

                return;
            }

            NetworkEvent.Type command;
            while ((command = connection.PopEvent(driver, out var reader)) != NetworkEvent.Type.Empty) {
                switch (command) {
                    case NetworkEvent.Type.Connect: {
                        Debug.Log($"Client: Successfully connected to {serverIP}:{serverPort}.");
                        var handshake = new ClientHandshakeEvent(ClientConfig.GameMode, new PlayerOptions {
                            CarColour  = ClientConfig.VehicleColour,
                            CarType    = ClientConfig.VehicleType,
                            PlayerName = ClientConfig.PlayerName,
                        });
                        SendEventToServer(handshake);
                        break;
                    }
                    case NetworkEvent.Type.Data: {
                        var readerContext = default(DataStreamReader.Context);
                        
                        while (reader.Length - reader.GetBytesRead(ref readerContext) > 0) {
                            var ev = (EventType) reader.ReadByte(ref readerContext);
                            HandleEvent(ev, reader, ref readerContext);
                        }

                        break;
                    }
                    case NetworkEvent.Type.Disconnect: {
                        Debug.Log("Client: Disconnected from the server.");
                        done       = true;
                        connection = default;
                        break;
                    }
                }
            }
        }

        public string GetServerIP() {
            return serverIP;
        }


        // Delegate event handlers
        public void OnSpaceEnter(int playerID, ushort spaceID) {
            var ev = new ClientSpaceEnterEvent(spaceID);
            SendEventToServer(ev);
            Debug.Log($"Someone entered the space #{spaceID}");
        }

        public void OnSpaceExit(int playerID, ushort spaceID) {
            var ev = new ClientSpaceExitEvent(spaceID);
            SendEventToServer(ev);
            Debug.Log($"Someone exited the space #{spaceID}");
        }

        public void OnTriggerGameStart() {
            var ev = new AdminClientStartGameEvent();
            SendEventToServer(ev);
        }

        private void SendEventToServer(Event ev) {
            eventQueue.Enqueue(ev);
        }

        private void HandleEvent(EventType eventType, DataStreamReader reader, ref DataStreamReader.Context readerContext) {
            Event ev;
            switch (eventType) {
                case EventType.ServerHandshake: {
                    ev = new ServerHandshakeEvent();
                    break;
                }
                case EventType.ServerLocationUpdate: {
                    ev = new ServerLocationUpdateEvent();
                    break;
                }
                case EventType.ServerPreRoundStartEvent: {
                    ev = new ServerPreRoundStartEvent();
                    break;
                }
                case EventType.ServerRoundStartEvent: {
                    ev = new ServerRoundStartEvent();
                    break;
                }
                case EventType.ServerRoundEndEvent: {
                    ev = new ServerRoundEndEvent();
                    break;
                }
                case EventType.ServerEliminatePlayersEvent: {
                    ev = new ServerEliminatePlayersEvent();
                    break;
                }
                case EventType.ServerDisconnectEvent: {
                    ev = new ServerDisconnectEvent();
                    break;
                }
                case EventType.ServerKeepAliveEvent: {
                    ev = new ServerKeepAlive();
                    break;
                }
                case EventType.ServerGameStartEvent: {
                    ev = new ServerGameStart();
                    break;
                }
                case EventType.ServerSpaceClaimedEvent: {
                    ev = new ServerSpaceClaimedEvent();
                    break;
                }
                case EventType.ServerGameEndEvent: {
                    ev = new ServerGameEndEvent();
                    break;
                }
                case EventType.ServerNewPlayerConnectedEvent: {
                    ev = new ServerNewPlayerConnectedEvent();
                    break;
                }
                default: {
                    Debug.Log($"Received an invalid event {eventType} from {serverIP}:{serverPort}.");
                    return;
                }
            }

            ev.Deserialise(reader, ref readerContext);

            ev.Handle(this, connection);
        }


        // Handle Event methods
        public void Handle(Event ev, NetworkConnection conn) {
            throw new ArgumentException("Client received an event that it cannot handle");
        }

        public void Handle(ServerHandshakeEvent ev, NetworkConnection conn) {
            Debug.Log($"Client: Received handshake back from {serverIP}:{serverPort}.");
            var timestamp = (ulong) DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var tickDelta = (timestamp - ev.Timestamp) / TickDuration; 
            tick = ev.BaseTick + tickDelta;
//            Debug.Log($"t = {tick}, timestamp = {ev.Timestamp}");
            var playerID = ev.PlayerID;

            ev.Apply(world);

            PlayerCountChangeEvent?.Invoke(world.GetNumPlayers());
            Debug.Log($"Client: My playerID is {playerID}");
        }

        public void Handle(ServerNewPlayerConnectedEvent ev, NetworkConnection conn) {
            Debug.Log($"Client: New player has connected to the game");

            if (ev.PlayerID != ClientConfig.PlayerID) world.CreatePlayer(ev.PlayerID, ev.PlayerOptions);
            PlayerCountChangeEvent?.Invoke(world.GetNumPlayers());
        }

        public void Handle(ServerGameStart ev, NetworkConnection conn) {
            Debug.Log($"Client: Received handshake back from {serverIP}:{serverPort}.");

            ev.SpawnPlayers(world);
            inGame = true;

            GameStartEvent?.Invoke(ev.FreeRoamLength, (ushort) ev.Length);
        }

        public void Handle(ServerLocationUpdateEvent ev, NetworkConnection conn) {
            ev.UpdateLocations(world);
        }

        public void Handle(ServerDisconnectEvent ev, NetworkConnection conn) {
            world.DestroyPlayer(ev.PlayerID);
            PlayerCountChangeEvent?.Invoke(world.GetNumPlayers());
            Debug.Log($"Client: Destroyed player {ev.PlayerID} due to disconnect.");
        }

        public void Handle(ServerPreRoundStartEvent ev, NetworkConnection conn) {
            PlayerCountChangeEvent?.Invoke(ev.PlayerCount);
            PreRoundStartEvent?.Invoke(ev.RoundNumber, ev.PreRoundLength, ev.RoundLength, ev.PlayerCount);
        }

        public void Handle(ServerRoundStartEvent ev, NetworkConnection conn) {
            RoundStartEvent?.Invoke(ev.RoundNumber, ev.Spaces);
        }

        public void Handle(ServerRoundEndEvent ev, NetworkConnection conn) {
            RoundEndEvent?.Invoke(ev.RoundNumber);
        }

        public void Handle(ServerEliminatePlayersEvent ev, NetworkConnection conn) {
            EliminatePlayersEvent?.Invoke(ev.RoundNumber, ev.Players);

            foreach (var playerID in ev.Players) {
                world.DestroyPlayer(playerID);
            }

            PlayerCountChangeEvent?.Invoke(world.GetNumPlayers());
        }

        public void Handle(ServerGameEndEvent ev, NetworkConnection conn) {
            GameEndEvent?.Invoke(ev.Winners);
        }

        public void Handle(ServerKeepAlive ev, NetworkConnection conn) {
            // Don't really need to do anything... Maybe a packet is needed to be sent back.
        }

        public void Handle(ServerSpaceClaimedEvent ev, NetworkConnection conn) {
            SpaceClaimedEvent?.Invoke(ev.PlayerID, ev.SpaceID);
        }
    }
}