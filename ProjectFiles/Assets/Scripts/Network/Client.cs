using System;
using Game;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using NetworkConnection = Unity.Networking.Transport.NetworkConnection;
using Object = UnityEngine.Object;
using UdpCNetworkDriver = Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket,Unity.Networking.Transport.DefaultPipelineStageCollection>;

namespace Network
{


    public interface IClient
    {
        bool Start(string ip = "127.0.0.1", ushort port = 25565);
        void Shutdown();
        void SendLocationUpdate();
        void HandleNetworkEvents();
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
            int playerID = world.ClientID;
            Transform transform = world.GetPlayerTransform(playerID);
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            using (var writer = ClientNetworkEvent.ClientLocationUpdate.GetWriter(30, Allocator.Temp))
            {
                writer.Write(position.x);
                writer.Write(position.y);
                writer.Write(position.z);
                writer.Write(rotation.x);
                writer.Write(rotation.y);
                writer.Write(rotation.z);
                writer.Write(rotation.w);
                
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
                        
                        using (var writer = ClientNetworkEvent.ClientHandshake.GetWriter(0, Allocator.Temp))
                        {
                            driver.Send(pipeline, connection, writer);
                        }
                        break;
                    }
                    case NetworkEvent.Type.Data:
                    {
                        var readerContext = default(DataStreamReader.Context);
                        var ev = (ServerNetworkEvent) reader.ReadByte(ref readerContext);
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
        
        private void HandleEvent(ServerNetworkEvent ev, DataStreamReader reader, DataStreamReader.Context readerContext)
        {
            switch (ev)
            {
                case ServerNetworkEvent.ServerHandshake:
                {

                    Debug.Log($"Client: Received handshake back from {serverIP}:{serverPort}.");

                    // Get player location from readerContext.
                    int playerID = reader.ReadByte(ref readerContext);

                    Vector3 position = new Vector3(
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext)
                    );

                    world.SpawnPlayer(playerID, position, true);
                    world.ClientID = playerID;

                    Debug.Log($"Client: Spawned myself (ID {playerID}) {position.x}, {position.y}, {position.z}.");

                    break;
                }
                case ServerNetworkEvent.ServerLocationUpdate:
                {
                    if (world.ClientID == -1) break;
                    
                    int length = reader.ReadByte(ref readerContext);
                    for (int i = 0; i < length; i++)
                    {
                        var playerID = reader.ReadByte(ref readerContext);
                        Vector3 position = new Vector3(
                            reader.ReadFloat(ref readerContext),
                            reader.ReadFloat(ref readerContext),
                            reader.ReadFloat(ref readerContext)
                        );
                        Quaternion rotation = new Quaternion(
                            reader.ReadFloat(ref readerContext),
                            reader.ReadFloat(ref readerContext),
                            reader.ReadFloat(ref readerContext),
                            reader.ReadFloat(ref readerContext)
                        );

                        if (playerID != world.ClientID)
                        {
                            Debug.Log($"Updating transforms for player {playerID}.");
                            
                            world.SetPlayerRotation(playerID, rotation);
                            world.SetPlayerPosition(playerID, position);
                        }
                    }

                    break;
                }
                case ServerNetworkEvent.SpawnPlayerEvent:
                {
                    int playerID = reader.ReadByte(ref readerContext);
                    
                    Vector3 position = new Vector3(
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext)

                    );
                    Quaternion rotation = new Quaternion(
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext),
                        reader.ReadByte(ref readerContext)
                    );

                    if (playerID != world.ClientID)
                    {
                        world.SpawnPlayer(playerID, position, false);
                        Debug.Log($"Spawned player with ID {playerID}");
                    }

                    break;
                }
                default:
                    Debug.Log($"Received an invalid event ({ev}) from {serverIP}:{serverPort}.");
                    break;
            }
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
        }
    }
}