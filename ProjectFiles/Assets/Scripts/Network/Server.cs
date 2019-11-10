using System;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using NetworkConnection = Unity.Networking.Transport.NetworkConnection; 
using UdpCNetworkDriver = Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket,Unity.Networking.Transport.DefaultPipelineStageCollection>;

namespace Network
{
    public class Server
    {
        public UdpCNetworkDriver Driver;
        private NativeList<NetworkConnection> connections;
        private NetworkPipeline pipeline;

        public string IP { get; private set; }
        public ushort Port { get; private set; }
        
        public Server()
        {
            // TODO: can simulate bad network conditions here by changing pipeline params
            // ReliableSequenced might not be the best choice 
            Driver = new UdpCNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
            pipeline = Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        }

        public bool Start(string ip = "0.0.0.0", ushort port = 25565)
        {
            IP = ip;
            Port = port;
            if (Driver.Bind(NetworkEndPoint.Parse(IP, Port)) != 0)
            {
                Debug.Log($"Server: Failed to bind to port {Port}. Is the port already in use?");
                return false;
            }
            Driver.Listen();
            Debug.Log($"Server listening at port {IP}:{Port}...");
            
            connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
            
            return true;
        }
        
        public void Shutdown()
        {
            Driver.Dispose();
            connections.Dispose();
        }

        public void HandleNetworkEvents()
        {
            Driver.ScheduleUpdate().Complete();
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
            while ((c = Driver.Accept()) != default(NetworkConnection))
            {
                connections.Add(c);
                var endpoint = Driver.RemoteEndPoint(c);
                Debug.Log($"Server: Accepted a connection from {endpoint.IpAddress()}:{endpoint.Port}.");
            }
        
            // Process events since the last update
            for (var i = 0; i < connections.Length; i++)
            {
                if (!connections[i].IsCreated) continue;
                var endpoint = Driver.RemoteEndPoint(connections[i]);
                NetworkEvent.Type command;
                while ((command = Driver.PopEventForConnection(connections[i], out var reader)) != NetworkEvent.Type.Empty)
                {
                    switch (command)
                    {
                        case NetworkEvent.Type.Data:
                        {
                            var readerContext = default(DataStreamReader.Context);
                            var ev = (ClientNetworkEvent) reader.ReadByte(ref readerContext);
                            HandleEvent(connections[i], endpoint, ev, reader, readerContext);
                            break;
                        }
                        case NetworkEvent.Type.Disconnect:
                            Debug.Log($"Server: {endpoint.IpAddress()}:{endpoint.Port} disconnected.");
                            connections[i] = default(NetworkConnection);
                            break;
                    }
                }
            }
        }
        
        private void HandleEvent(NetworkConnection connection, NetworkEndPoint endpoint, ClientNetworkEvent ev, 
                                 DataStreamReader reader, DataStreamReader.Context readerContext)
        {
            switch (ev)
            {
                case ClientNetworkEvent.ClientHandshake:
                    var number = reader.ReadUInt(ref readerContext);
                    Debug.Log($"Server: Received {number} from {endpoint.IpAddress()}:{endpoint.Port}.");
                    using (var writer = new DataStreamWriter(16, Allocator.Temp))
                    {
                        writer.Write((byte) ServerNetworkEvent.ServerHandshake);
                        writer.Write(number + 2);
                        Driver.Send(pipeline, connection, writer);
                    }
                    break;
                case ClientNetworkEvent.ClientLocationUpdate:
                    break;
                default:
                    Debug.Log($"Received an invalid event ({ev}) from {endpoint.IpAddress()}:{endpoint.Port}.");
                    break;
            }
        }
    }
}