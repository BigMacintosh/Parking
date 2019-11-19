using System.Collections.Generic;
using Game;
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
        private UdpCNetworkDriver Driver;
        private NativeList<NetworkConnection> connections;
        private NetworkPipeline pipeline;
        private World world;

        private Dictionary<int, int> connectionPlayerIDs;

        public ServerConfig Config { get; private set; }
        private string IP => Config.IpAddress;
        private ushort Port => Config.Port;
        
        public Server(World world, ServerConfig config)
        {
            this.world = world;
            Config = config;
            connections = new NativeList<NetworkConnection>(Config.MaxPlayers, Allocator.Persistent);
            connectionPlayerIDs = new Dictionary<int, int>();
            
            // TODO: can simulate bad network conditions here by changing pipeline params
            // ReliableSequenced might not be the best choice 
            Driver = new UdpCNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
            pipeline = Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        }

        public bool Start()
        {
            if (Driver.Bind(NetworkEndPoint.Parse(IP, Port)) != 0)
            {
                Debug.Log($"Server: Failed to bind to port {IP}:{Port}. Is the port already in use?");
                Shutdown();
                return false;
            }
            Driver.Listen();
            Debug.Log($"Server: Listening at port {IP}:{Port}...");
            
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
                if (!connections[i].IsCreated)
                {
                    continue;
                }
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
                            HandleEvent(connections[i], endpoint, ev, reader, readerContext, i);
                            break;
                        }
                        case NetworkEvent.Type.Disconnect:
                            Debug.Log($"Server: {endpoint.IpAddress()}:{endpoint.Port} disconnected.");
                            connections[i] = default(NetworkConnection);

                            var playerID = connectionPlayerIDs[i];
                            world.DestroyPlayer(playerID);
                            connectionPlayerIDs.Remove(i);
                            Debug.Log($"Server: Destroyed player { playerID } due to disconnect.");
                            
                            break;
                    }
                }
            }
        }
        
        private void HandleEvent(NetworkConnection connection, NetworkEndPoint endpoint, ClientNetworkEvent ev, 
                                 DataStreamReader reader, DataStreamReader.Context readerContext, int connectionID)
        {
            switch (ev)
            {
                case ClientNetworkEvent.ClientHandshake:
                {
                    Debug.Log($"Server: Received handshake from {endpoint.IpAddress()}:{endpoint.Port}.");

                    using (var writer = new DataStreamWriter(30, Allocator.Temp))
                    {
                        // Get a player id
                        int playerID = world.SpawnPlayer();
                        connectionPlayerIDs.Add(connectionID, playerID);

                        // Get spawn location
                        Transform transform = world.GetPlayerTransform(playerID);
                        var position = transform.position;

                        writer.Write((byte) ServerNetworkEvent.ServerHandshake);
                        writer.Write((byte) playerID);
                        writer.Write(position.x);
                        writer.Write(position.y);
                        writer.Write(position.z);

                        Driver.Send(pipeline, connection, writer);
                    }

                    break;
                }
                case ClientNetworkEvent.ClientLocationUpdate:
                {
                    // Get player location from readerContext.
                    int playerID = reader.ReadByte(ref readerContext);

                    Vector3 newPosition = new Vector3(
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext)
                    );
                    Quaternion newRotation = new Quaternion(
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext)
                    );

                    world.SetPlayerPosition(playerID, newPosition);
                    world.SetPlayerRotation(playerID, newRotation);

                    Debug.Log($"Client: Update player location (ID {playerID}) {newPosition.x}, {newPosition.y}, {newPosition.z}.");

                    break;
                }
                default:
                    Debug.Log($"Received an invalid event ({ev}) from {endpoint.IpAddress()}:{endpoint.Port}.");
                    break;
            }
        }
    }
}