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
using Object = UnityEngine.Object;
using UdpCNetworkDriver = Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket,Unity.Networking.Transport.DefaultPipelineStageCollection>;

namespace Network
{
    
    public delegate void GameStartDelegate(int nPlayers);
    
    public delegate void PreRoundStartDelegate(
        int roundNumber, int preRoundLength, int roundLength, int nPlayers, List<byte> spacesActive);
    
    public delegate void RoundStartDelegate(int roundNumber);
    
    public delegate void RoundEndDelegate(int roundNumber);
    
    public delegate void EliminatePlayersDelegate(int roundNumber, List<int> eliminatedPlayers);


    public interface IClient
    {
        bool Start(string ip = "127.0.0.1", ushort port = 25565);
        void Shutdown();
        void SendLocationUpdate();
        void HandleNetworkEvents();
        event GameStartDelegate GameStartEvent;
        event PreRoundStartDelegate PreRoundStartEvent;
        event RoundStartDelegate RoundStartEvent;
        event RoundEndDelegate RoundEndEvent;
        event EliminatePlayersDelegate EliminatePlayersEvent;
    }

    public class Client : IClient
    {
        private UdpCNetworkDriver driver;
        private NetworkConnection connection;
        private NetworkPipeline pipeline;
        private World world;

        private string serverIP;
        private ushort serverPort;

        private bool done;

        public Client(World world)
        {
            driver = new UdpCNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
            pipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            done = false;
            this.world = world;
        }

        public bool Start(string ip = "127.0.0.1", ushort port = 25565)
        {
            serverIP = ip;
            serverPort = port;
            
            Debug.Log($"Client: Connecting to {serverIP}:{serverPort}...");
            
            var endpoint = NetworkEndPoint.Parse(serverIP, serverPort);
            connection = driver.Connect(endpoint);
            return true;
        }
        
        public void Shutdown()
        {
            Debug.Log("Client: Shutting down.");
            done = true;
            connection.Close(driver);
            driver.Dispose();
        }

        public void SendLocationUpdate()
        {
            var locationUpdate = new ClientLocationUpdateEvent(world);
            using (var writer = new DataStreamWriter(locationUpdate.Length, Allocator.Temp))
            {
                locationUpdate.Serialise(writer);
                driver.Send(pipeline, connection, writer);
            }
        }

        public void HandleNetworkEvents()
        {
            driver.ScheduleUpdate().Complete();
            
            // Check connection is valid
            if (!connection.IsCreated)
            {
                if (!done)
                {
                    Debug.Log($"Client: Something went wrong when connecting to {serverIP}:{serverPort}.");
                }
                
                return;
            }

            NetworkEvent.Type command;
            while ((command = connection.PopEvent(driver, out var reader)) != NetworkEvent.Type.Empty)
            {
                switch (command)
                {
                    case NetworkEvent.Type.Connect:
                    {
                        Debug.Log($"Client: Successfully connected to {serverIP}:{serverPort}.");
                        var handshake = new ClientHandshakeEvent();
                        using (var writer = new DataStreamWriter(handshake.Length, Allocator.Temp))
                        {
                            handshake.Serialise(writer);
                            driver.Send(pipeline, connection, writer);
                        }
                        break;
                    }
                    case NetworkEvent.Type.Data:
                    {
                        var readerContext = default(DataStreamReader.Context);
                        var ev = (EventType) reader.ReadByte(ref readerContext);
                        HandleEvent(ev, reader, readerContext);
                        break;
                    }
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log($"Client: Disconnected from the server.");
                        connection = default(NetworkConnection);
                        break;
                }
            }
        }

        

        private void HandleEvent(EventType eventType, DataStreamReader reader, DataStreamReader.Context readerContext)
        {
            Event ev;
            switch (eventType)
            {
                case EventType.ServerHandshake:
                {
                    ev = new ServerHandshakeEvent();
                    break;
                }
                case EventType.ServerLocationUpdate:
                {
                    ev = new ServerLocationUpdateEvent();
                    break;
                }
                case EventType.ServerSpawnPlayerEvent:
                {
                    ev = new ServerSpawnPlayerEvent();
                    break;
                }
                default:
                    Debug.Log($"Received an invalid event {eventType} from {serverIP}:{serverPort}.");
                    return;
            }
            ev.Deserialise(reader, ref readerContext);
            
            ev.Handle(this, connection);
        }

        public static IClient getDummyClient(World world)
        {
            return new DummyClient(world);
        }
        
        private class DummyClient : IClient
        {
            private World world;
            private int playerID;
            
            
            public DummyClient(World world)
            {
                this.world = world;
            }
            public bool Start(string ip, ushort port)
            {
                world.ClientID = 0;
                world.SpawnPlayer(world.ClientID);
                world.SetPlayerControllable(world.ClientID);
                return true;
            }

            public void Shutdown()
            {
                
            }

            public void SendLocationUpdate()
            {
                
            }

            public void HandleNetworkEvents()
            {
                
            }
            public event GameStartDelegate GameStartEvent;
            public event PreRoundStartDelegate PreRoundStartEvent;
            public event RoundStartDelegate RoundStartEvent;
            public event RoundEndDelegate RoundEndEvent;
            public event EliminatePlayersDelegate EliminatePlayersEvent;
        }
        
        public void Handle(Event ev, NetworkConnection conn) {
            throw new ArgumentException("Client received an event that it cannot handle");
        }
 
        public void Handle(ServerHandshakeEvent ev, NetworkConnection conn)
        {                    
            Debug.Log($"Client: Received handshake back from {serverIP}:{serverPort}.");
            var playerID = ev.PlayerID;
            var position = ev.Position;
            
            world.SpawnPlayer(playerID, position, true);
            world.ClientID = playerID;

            Debug.Log($"Client: Spawned myself (ID {playerID}) {position.x}, {position.y}, {position.z}.");
        }

        public void Handle(ServerSpawnPlayerEvent ev, NetworkConnection conn)
        {
            var playerID = ev.PlayerID;
            var position = ev.Position;
            var rotation = ev.Rotation;
            
            if (playerID != world.ClientID)
            {
                world.SpawnPlayer(playerID, position, false);
                world.SetPlayerRotation(playerID, rotation);
                Debug.Log($"Spawned player with ID {playerID}");
            }
        }

        public void Handle(ServerLocationUpdateEvent ev, NetworkConnection conn)
        {
            if (world.ClientID == -1) return;
            foreach (var playerID in ev.Positions.Keys.Where(k => k != world.ClientID))
            {
                world.SetPlayerRotation(playerID, ev.Rotations[playerID]);
                world.SetPlayerPosition(playerID, ev.Positions[playerID]);
                world.SetPlayerVelocity(playerID, ev.Velocities[playerID]);
                world.SetPlayerAngularVelocity(playerID, ev.AngularVelocities[playerID]);
            }
        }
        public event GameStartDelegate GameStartEvent;
        public event PreRoundStartDelegate PreRoundStartEvent;
        public event RoundStartDelegate RoundStartEvent;
        public event RoundEndDelegate RoundEndEvent;
        public event EliminatePlayersDelegate EliminatePlayersEvent;
    }
}