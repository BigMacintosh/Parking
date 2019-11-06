using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using NetworkConnection = Unity.Networking.Transport.NetworkConnection;
using UdpCNetworkDriver = Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket,Unity.Networking.Transport.DefaultPipelineStageCollection>;

namespace Game
{
    public class ClientGameLoop : IGameLoop
    {
    
        public UdpCNetworkDriver Driver;
        public NetworkConnection Connection;
        public bool Done;
        private NetworkPipeline pipeline;
    
        // Update is called once per frame
        public bool Init(string[] args)
        {
            Driver = new UdpCNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
            pipeline = Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            Connection = default(NetworkConnection);

            // TODO: make server endpoint configurable
#if UNITY_EDITOR
            string ipAddress = "127.0.0.1";
#else
        string ipAddress = "18.191.231.10";
#endif
        
            var endpoint = NetworkEndPoint.Parse(ipAddress, 25565);
            Connection = Driver.Connect(endpoint);
        
            Debug.Log("Client: ClientGameLoop Init");
            return true;
        }

        public void Shutdown()
        {
            Driver.Dispose();
        }
        
        public void Update()
        {
//            Debug.Log("ClientGameLoop: Update() call");
            Driver.ScheduleUpdate().Complete();

            if (!Connection.IsCreated)
            {
                if (!Done)
                {
                    Debug.Log($"Client: Something went wrong when connecting to .");
                }
                return;
            }

            DataStreamReader reader;
            NetworkEvent.Type command;
            while ((command = Connection.PopEvent(Driver, out reader)) != NetworkEvent.Type.Empty)
            {
                Debug.Log("Client: Command received");
                if (command == NetworkEvent.Type.Connect)
                {
                    Debug.Log($"Client: Successfully connected to server.");
                
                    var value = 1;
                    using (var writer = new DataStreamWriter(4, Allocator.Temp))
                    {
                        writer.Write(value);
                        Driver.Send(pipeline, Connection, writer);
                    }
                }
                else if (command == NetworkEvent.Type.Data)
                {
                    Debug.Log("Client: Data received");
                    var readerContext = default(DataStreamReader.Context);
                    var value = reader.ReadUInt(ref readerContext);
                    Debug.Log($"Client: Got the value {value} back from the server.");
                    Done = true;
                    Connection.Disconnect(Driver);
                    Connection = default(NetworkConnection);
                }
                else if (command == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log($"Client: Disconnected from the server.");
                    Connection = default(NetworkConnection);
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
