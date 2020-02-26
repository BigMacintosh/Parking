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

    public interface IClient
    {
        bool Start(ushort port = 25565);
        void Shutdown();
        void SendLocationUpdate();
        void HandleNetworkEvents();
        string GetServerIP();
        event GameStartDelegate GameStartEvent;
        event PreRoundStartDelegate PreRoundStartEvent;
        event RoundStartDelegate RoundStartEvent;
        event RoundEndDelegate RoundEndEvent;
        event SpaceClaimedDelegate SpaceClaimedEvent;
        event EliminatePlayersDelegate EliminatePlayersEvent;
        event PlayerCountChangeDelegate PlayerCountChangeEvent;
        event GameEndDelegate GameEndEvent;
        void OnSpaceEnter(int playerID, ushort spaceID);
        void OnSpaceExit(int playerID, ushort spaceID);
        void OnTriggerGameStart();
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
        private bool inGame;
        
        public event GameStartDelegate GameStartEvent;
        public event PreRoundStartDelegate PreRoundStartEvent;
        public event RoundStartDelegate RoundStartEvent;
        public event RoundEndDelegate RoundEndEvent;
        public event EliminatePlayersDelegate EliminatePlayersEvent;
        public event PlayerCountChangeDelegate PlayerCountChangeEvent;
        public event GameEndDelegate GameEndEvent;
        public event SpaceClaimedDelegate SpaceClaimedEvent;
        

        public Client(World world)
        {
            driver = new UdpCNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
            pipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            done = false;
            this.world = world;
        }
        
        public static IClient getDummyClient(World world)
        {
            return new DummyClient(world);
        }

        public bool Start(ushort port = 25565)
        {
            serverIP = ClientConfig.ServerIP;
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

        private void sendEventToServer(Event ev)
        {
            using (var writer = new DataStreamWriter(ev.Length, Allocator.Temp))
            {
                ev.Serialise(writer);
                driver.Send(pipeline, connection, writer);
            }
        }
        
        public void SendLocationUpdate()
        {
            // Don't send location if not in game
            if (!inGame) return;
            // Don't send location if you are admin client
            if (ClientConfig.GameMode == GameMode.AdminMode) return;
            
            var locationUpdate = new ClientLocationUpdateEvent(world);
            sendEventToServer(locationUpdate);
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
                        var handshake = new ClientHandshakeEvent(ClientConfig.GameMode);
                        sendEventToServer(handshake);
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
                        done = true;
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
                case EventType.ServerPreRoundStartEvent:
                {
                    ev = new ServerPreRoundStartEvent();
                    break;
                }
                case EventType.ServerRoundStartEvent:
                {
                    ev = new ServerRoundStartEvent();
                    break;
                }
                case EventType.ServerRoundEndEvent:
                {
                    ev = new ServerRoundEndEvent();
                    break;
                }
                case EventType.ServerEliminatePlayersEvent:
                {
                    ev = new ServerEliminatePlayersEvent();
                    break;
                }
                case EventType.ServerDisconnectEvent:
                {
                    ev = new ServerDisconnectEvent();
                    break;
                }
                case EventType.ServerKeepAliveEvent:
                {
                    ev = new ServerKeepAlive();
                    break;
                }
                case EventType.ServerGameStartEvent:
                {
                    ev = new ServerGameStart();
                    break;
                }
                case EventType.ServerSpaceClaimedEvent:
                {
                    ev = new ServerSpaceClaimedEvent();
                    break;
                }
                default:
                    Debug.Log($"Received an invalid event {eventType} from {serverIP}:{serverPort}.");
                    return;
            }
            ev.Deserialise(reader, ref readerContext);
            
            ev.Handle(this, connection);
        }
        
        public String GetServerIP()
        {
            return serverIP;
        }

        // Handle Event methods
        public void Handle(Event ev, NetworkConnection conn) {
            throw new ArgumentException("Client received an event that it cannot handle");
        }
 
        public void Handle(ServerHandshakeEvent ev, NetworkConnection conn)
        {                    
            Debug.Log($"Client: Received handshake back from {serverIP}:{serverPort}.");
            var playerID = ev.PlayerID;
            world.ClientID = playerID;
            
            Debug.Log($"Client: My playerID is {playerID}");
        }
        
        public void Handle(ServerGameStart ev, NetworkConnection conn)
        {                    
            Debug.Log($"Client: Received handshake back from {serverIP}:{serverPort}.");

            ev.SpawnPlayers(world);

            if (ClientConfig.GameMode == GameMode.PlayerMode)
            {
                world.SetPlayerControllable(world.ClientID);
            }

            inGame = true;
            PlayerCountChangeEvent?.Invoke(world.GetNumPlayers());
            GameStartEvent?.Invoke(ev.FreeRoamLength, (ushort) ev.Length);
            
        }

        public void Handle(ServerLocationUpdateEvent ev, NetworkConnection conn)
        {
            ev.UpdateLocations(world);
        }

        public void Handle(ServerDisconnectEvent ev, NetworkConnection conn)
        {
            if (inGame) world.DestroyPlayer(ev.PlayerID);
            PlayerCountChangeEvent?.Invoke(world.GetNumPlayers());
            Debug.Log($"Client: Destroyed player { ev.PlayerID } due to disconnect.");
        }

        public void Handle(ServerPreRoundStartEvent ev, NetworkConnection conn)
        {
            PlayerCountChangeEvent?.Invoke(ev.PlayerCount);
            PreRoundStartEvent?.Invoke(ev.RoundNumber, ev.PreRoundLength, ev.RoundLength, ev.PlayerCount);
        }

        public void Handle(ServerRoundStartEvent ev, NetworkConnection conn)
        {
            RoundStartEvent?.Invoke(ev.RoundNumber, ev.Spaces);
        }

        public void Handle(ServerRoundEndEvent ev, NetworkConnection conn)
        {
            RoundEndEvent?.Invoke(ev.RoundNumber);
        }

        public void Handle(ServerEliminatePlayersEvent ev, NetworkConnection conn)
        {
            
            EliminatePlayersEvent?.Invoke(ev.RoundNumber, ev.Players);

//            if (ev.Players.Contains(world.ClientID))
//            {
//                Shutdown();
//            }
        }
        
        public void Handle(ServerGameEndEvent ev, NetworkConnection conn)
        {
            GameEndEvent?.Invoke();
        }
        
        public void Handle(ServerKeepAlive ev, NetworkConnection conn)
        {
            // Don't really need to do anything... Maybe a packet is needed to be sent back.
        }
        

        public void Handle(ServerSpaceClaimedEvent ev, NetworkConnection conn)
        {
            SpaceClaimedEvent?.Invoke(ev.PlayerID, ev.SpaceID);
        }
        
        
        // Delegate event handlers
        public void OnSpaceEnter(int playerID, ushort spaceID)
        {
            ClientSpaceEnterEvent ev = new ClientSpaceEnterEvent(spaceID);
            sendEventToServer(ev);
            Debug.Log($"Someone entered the space #{spaceID}");   
        }

        public void OnSpaceExit(int playerID, ushort spaceID)
        {
            ClientSpaceExitEvent ev = new ClientSpaceExitEvent(spaceID);
            sendEventToServer(ev);
            Debug.Log($"Someone exited the space #{spaceID}");
        }

        public void OnTriggerGameStart()
        {
            AdminClientStartGameEvent ev = new AdminClientStartGameEvent();
            sendEventToServer(ev);
        }
        
    }
}