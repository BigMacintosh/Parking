using System;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using NetworkConnection = Unity.Networking.Transport.NetworkConnection; 
using UdpCNetworkDriver = Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket,Unity.Networking.Transport.DefaultPipelineStageCollection>;

namespace Network
{
    public class Client
    {
        private UdpCNetworkDriver driver;
        private NetworkConnection connection;
        private NetworkPipeline pipeline;

        private string serverIP;
        private ushort serverPort;

        private bool done;

        public Client()
        {
            driver = new UdpCNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
            pipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            done = false;
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
                        var value = 1;
                        using (var writer = new DataStreamWriter(16, Allocator.Temp))
                        {
                            writer.Write((byte) ClientNetworkEvent.ClientHandshake);
                            writer.Write(value);
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
                    var number = reader.ReadUInt(ref readerContext);
                    Debug.Log($"Client: Received {number} back from {serverIP}:{serverPort}.");

                    done = true;
                    connection.Disconnect(driver);
                    connection = default(NetworkConnection);
                    
                    break;
                case ServerNetworkEvent.ServerLocationUpdate:
                    break;
                default:
                    Debug.Log($"Received an invalid event ({ev}) from {serverIP}:{serverPort}.");
                    break;
            }
        }
    }
}