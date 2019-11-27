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

        public void SendLocationUpdates()
        {
            var numConns = connections.Length;

            var length = 0;
            for (int i = 0; i < numConns; i++)
            {
                if (connectionPlayerIDs.ContainsKey(connections[i].InternalId))
                {
                    ++length;
                }
            }


            if (length == 0) return;
            
            using (var writer = ServerNetworkEvent.ServerLocationUpdate.GetWriter(1  + length * 29, Allocator.Temp))
            {
                
                writer.Write((byte) length);

                for (int i = 0; i < numConns; i++)
                {
                    var connectionID = connections[i].InternalId;
                    
                    if (connectionPlayerIDs.ContainsKey(connectionID))
                    {
                        var playerID = connectionPlayerIDs[connectionID];

                        var transform = world.GetPlayerTransform(playerID);

                        writer.Write((byte) playerID);
                    
                        var position = transform.position;
                        writer.Write(position.x);
                        writer.Write(position.y);
                        writer.Write(position.z);
                    
                        var rotation = transform.rotation;
                        writer.Write(rotation.x);
                        writer.Write(rotation.y);
                        writer.Write(rotation.z);
                        writer.Write(rotation.w);       
                    }
                }

                for (int i = 0; i < numConns; i++)
                {
                    Driver.Send(pipeline, connections[i], writer);
                }
            }
        }

        public void HandleNetworkEvents()
        {
            Driver.ScheduleUpdate().Complete();
            // Clean up connections
            for (var i = 0; i < connections.Length; i++)
            {
                if (!connections[i].IsCreated)
                {
                    var connectionID = connections[i].InternalId;
                    
                    // Remove from world and player id mapping
                    var playerID = connectionPlayerIDs[connectionID];
                    world.DestroyPlayer(playerID);
                    connectionPlayerIDs.Remove(connectionID);
                            
                    // Destroy the actual network connection
                    connections[i] = default(NetworkConnection);
                            
                    Debug.Log($"Server: Destroyed player { playerID } due to disconnect.");
                    
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
                var connection = connections[i];
                var connectionID = connection.InternalId;
                
                if (!connection.IsCreated)
                {
                    continue;
                }
                
                var endpoint = Driver.RemoteEndPoint(connection);
                NetworkEvent.Type command;
                while ((command = Driver.PopEventForConnection(connection, out var reader)) != NetworkEvent.Type.Empty)
                {
                    switch (command)
                    {
                        case NetworkEvent.Type.Data:
                        {
                            var readerContext = default(DataStreamReader.Context);
                            var ev = (ClientNetworkEvent) reader.ReadByte(ref readerContext);
                            HandleEvent(connections[i], endpoint, ev, reader, readerContext, connectionID);
                            break;
                        }
                        case NetworkEvent.Type.Disconnect:
                            Debug.Log($"Server: {endpoint.IpAddress()}:{endpoint.Port} disconnected.");

                            break;
                    }
                }
            }
        }

        private void SendSpawnPlayer(int playerID, int recipientID)
        {
            using (var writer = ServerNetworkEvent.SpawnPlayerEvent.GetWriter(29, Allocator.Temp))
            {
                // Get spawn location
                Transform transform = world.GetPlayerTransform(playerID);
                var position = transform.position;
                var rotation = transform.rotation;
                        
                writer.Write((byte) playerID);
                writer.Write(position.x);
                writer.Write(position.y);
                writer.Write(position.z);
                writer.Write(rotation.x);
                writer.Write(rotation.y);
                writer.Write(rotation.z);
                writer.Write(rotation.z);

                Driver.Send(pipeline, connections[recipientID], writer);
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

                    using (var writer = ServerNetworkEvent.ServerHandshake.GetWriter(30, Allocator.Temp))
                    {
                        
                        // Get a player id
                        int playerID = world.SpawnPlayer();
                        connectionPlayerIDs.Add(connectionID, playerID);

                        // Get spawn location
                        Transform transform = world.GetPlayerTransform(playerID);
                        var position = transform.position;
                        
                        writer.Write((byte) playerID);
                        writer.Write(position.x);
                        writer.Write(position.y);
                        writer.Write(position.z);

                        Driver.Send(pipeline, connection, writer);
                        
                        // tell other clients you have spawned
                        for (int i = 0; i < connections.Length; i++)
                        {
                            var internalID = connections[i].InternalId;
                            if (internalID != connectionID)
                            {
                                SendSpawnPlayer(playerID, internalID);
                            }
                        }
                        
                        // tell this client about the other clients
                        for (int i = 0; i < connections.Length; i++)
                        {
                            var internalID = connections[i].InternalId;
                            if (internalID != connectionID)
                            {
                                SendSpawnPlayer(connectionPlayerIDs[internalID], connectionID);
                            }
                        }
                    }

                    break;
                }
                case ClientNetworkEvent.ClientLocationUpdate:
                {
                    // Get player location from readerContext.
                    int playerID = connectionPlayerIDs[connectionID];

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

                    Debug.Log($"Server: Update player location (ID {playerID}) {newPosition.x}, {newPosition.y}, {newPosition.z}.");

                    break;
                }
                default:
                    Debug.Log($"Received an invalid event ({ev}) from {endpoint.IpAddress()}:{endpoint.Port}.");
                    break;
            }
        }
    }
}