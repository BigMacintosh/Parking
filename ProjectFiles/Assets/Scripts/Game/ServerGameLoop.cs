using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using NetworkConnection = Unity.Networking.Transport.NetworkConnection; 
using UdpCNetworkDriver = Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket,Unity.Networking.Transport.DefaultPipelineStageCollection>;


namespace Game
{
    public class ServerGameLoop : IGameLoop
    {
        public UdpCNetworkDriver Driver;
        private NativeList<NetworkConnection> connections;
        private NetworkPipeline pipeline;
    
        public bool Init(string[] args)
        {
            // TODO: can simulate bad network conditions here by changing pipeline params
            // ReliableSequenced might not be the best choice 
            Driver = new UdpCNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
            pipeline = Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            if (Driver.Bind(NetworkEndPoint.Parse("0.0.0.0", 25565)) != 0)
            {
                Debug.Log("Server: Failed to bind to port 25565");
                return false;
            }
            else
            {
                Driver.Listen();
            }
            connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

            Debug.Log("Server: ServerGameLoop Init");
            return true;
        }

        public void Shutdown()
        {
            Driver.Dispose();
            connections.Dispose();
        }

        public void Update()
        {
            Debug.Log("ServerGameLoop: Update() call");
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
                            var readerContext = default(DataStreamReader.Context);
                            var number = reader.ReadUInt(ref readerContext);
                            Debug.Log($"Server: Received number {number} from {connections[i].InternalId}");
                            number += 2;
                            using (var writer = new DataStreamWriter(4, Allocator.Temp))
                            {
                                writer.Write(number);
                                Driver.Send(pipeline, connection, writer);
                            }
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

        public void FixedUpdate()
        {
        
        }

        public void LateUpdate()
        {
        
        }
    }
}
