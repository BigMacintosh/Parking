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
        
        public ServerConfig Config { get; private set; }
        private string IP => Config.IpAddress;
        private ushort Port => Config.Port;
        
        public Server(World world, ServerConfig config)
        {
            this.world = world;
            Config = config;
            connections = new NativeList<NetworkConnection>(Config.MaxPlayers, Allocator.Persistent);
            
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

            var length = world.GetNumPlayers();
            if (length == 0) return;
            
            using (var writer = ServerNetworkEvent.ServerLocationUpdate.GetWriter(1  + length * 53, Allocator.Temp))
            {
                
                writer.Write((byte) length);

                foreach (var playerID in world.PlayerIDs)
                {
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

                    var velocity = world.GetPlayerVelocity(playerID);
                    writer.Write(velocity.x);
                    writer.Write(velocity.y);
                    writer.Write(velocity.z);

                    var angularVelocity = world.GetPlayerAngularVelocity(playerID);
                    writer.Write(angularVelocity.x);
                    writer.Write(angularVelocity.y);
                    writer.Write(angularVelocity.z);
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
                    var playerID = connections[i].InternalId;
                    
                    // Remove from world and player id mapping
                    world.DestroyPlayer(playerID);

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

        private void SendSpawnPlayer(int playerID, NetworkConnection connection)
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
                writer.Write(rotation.w);

                Driver.Send(pipeline, connection, writer);
            }
        }
        
        private void HandleEvent(NetworkConnection connection, NetworkEndPoint endpoint, ClientNetworkEvent ev, 
                                 DataStreamReader reader, DataStreamReader.Context readerContext, int playerID)
        {
            switch (ev)
            {
                case ClientNetworkEvent.ClientHandshake:
                {
                    Debug.Log($"Server: Received handshake from {endpoint.IpAddress()}:{endpoint.Port}.");

                    using (var writer = ServerNetworkEvent.ServerHandshake.GetWriter(30, Allocator.Temp))
                    {
                        // Get a player id
                        world.SpawnPlayer(playerID);

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
                            var ID = connections[i].InternalId;
                            if (ID != playerID)
                            {
                                Debug.Log($"Spawning new player {playerID} on client {ID}");
                                SendSpawnPlayer(playerID, connections[i]);
                            }
                        }
                        
                        // tell this client about the other clients
                        foreach (var ID in world.PlayerIDs)
                        {
                            if (ID != playerID)
                            {
                                Debug.Log($"Spawning pre-existing player {ID} on {playerID}.");
                                SendSpawnPlayer(ID, connection);
                            }
                        }
                    }

                    break;
                }
                case ClientNetworkEvent.ClientLocationUpdate:
                {
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
                    Vector3 newVelocity = new Vector3(
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext)
                    );
                    Vector3 newAngularVelocity = new Vector3(
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext),
                        reader.ReadFloat(ref readerContext)
                    );

                    world.SetPlayerPosition(playerID, newPosition);
                    world.SetPlayerRotation(playerID, newRotation);
                    world.SetPlayerVelocity(playerID, newVelocity);
                    world.SetPlayerAngularVelocity(playerID, newAngularVelocity);
                    
                    break;
                }
                default:
                    Debug.Log($"Received an invalid event ({ev}) from {endpoint.IpAddress()}:{endpoint.Port}.");
                    break;
            }
        }
    }
}