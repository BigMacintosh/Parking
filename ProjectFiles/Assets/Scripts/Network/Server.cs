using System;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEditor.MemoryProfiler;
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
        
        public Server()
        {
            // TODO: can simulate bad network conditions here by changing pipeline params
            // ReliableSequenced might not be the best choice 
            Driver = new UdpCNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
            pipeline = Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            
            
        }

        public bool Start()
        {
            // TODO: add network configuration
            if (Driver.Bind(NetworkEndPoint.Parse("0.0.0.0", 25565)) != 0)
            {
                Debug.Log("Server: Failed to bind to port 25565");
                return false;
            }
            Driver.Listen();
            Debug.Log("Server listening at port 25565");
            
            connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
            
            return true;
        }
        
        public void Shutdown()
        {
            Driver.Dispose();
            connections.Dispose();
        }

        public void SomeFuckingFunctionTODOThinkOfNameWeAgreeOn()
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
                Debug.Log("Server: Accepted a connection");
            }
        
            // Process events since the last update
            DataStreamReader reader;
            for (var i = 0; i < connections.Length; i++)
            {
                if (!connections[i].IsCreated) continue;

                NetworkEvent.Type command;
                var connection = connections[i];
                while ((command = Driver.PopEventForConnection(connection, out reader)) != NetworkEvent.Type.Empty)
                {
                    switch (command)
                    {
                        case NetworkEvent.Type.Data:
                        {
//                            var readerContext = default(DataStreamReader.Context);
//                            var number = reader.ReadUInt(ref readerContext);
//                            Debug.Log($"Server: Received number {number} from {connections[i].InternalId}");
//                            number += 2;
//                            using (var writer = new DataStreamWriter(4, Allocator.Temp))
//                            {
//                                writer.Write(number);
//                                Driver.Send(pipeline, connection, writer);
//                            }
//                            break;
                            var readerContext = default(DataStreamReader.Context);
                            var ev = (ClientNetworkEvent) reader.ReadByte(ref readerContext);
                            ProcessEvent(connection, ev, reader, readerContext);
                            break;
                        }
                        case NetworkEvent.Type.Disconnect:
                            Debug.Log($"Server: Client disconnected");
                            connections[i] = default(NetworkConnection);
                            break;
                    }
                }
            }
        }
        
        private void ProcessEvent(NetworkConnection connection, ClientNetworkEvent ev, DataStreamReader reader, DataStreamReader.Context readerContext)
        {
            switch (ev)
            {
                case ClientNetworkEvent.ClientHandshake:
                    var number = reader.ReadUInt(ref readerContext);
                    Debug.Log($"Server: Received {number} from ID#{connection.InternalId}");
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
                    throw new ArgumentOutOfRangeException(nameof(ev), ev, null);
            }
        }
    }
}