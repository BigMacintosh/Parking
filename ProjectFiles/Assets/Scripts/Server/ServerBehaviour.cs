using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using NetworkConnection = Unity.Networking.Transport.NetworkConnection; 
using UdpCNetworkDriver = Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket,Unity.Networking.Transport.DefaultPipelineStageCollection>;

public class ServerBehaviour : MonoBehaviour
{
    public UdpCNetworkDriver Driver;
    private NativeList<NetworkConnection> connections;
    private NetworkPipeline pipeline;
    
    // Start is called before the first frame update
    internal void Start()
    {
        // TODO: can simulate bad network conditions here by changing pipeline params
        Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        pipeline = Driver.CreatePipeline();
        if (Driver.Bind(NetworkEndPoint.Parse("0.0.0.0", 25565)) != 0)
        {
            Debug.Log("Server: Failed to bind to port 25565");
        }
        else
        {
            Driver.Listen();
        }
        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    // Update is called once per frame
    internal void Update()
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
                        var readerContext = default(DataStreamReader.Context);
                        var number = reader.ReadUInt(ref readerContext);
                        Debug.Log($"Server: Received number {number} from {connections[i].InternalId}");
                        number += 2;
                        using (var writer = new DataStreamWriter(4, Allocator.Temp))
                        {
                            writer.Write(number);
                            connection.Send(Driver, writer);
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
    
    // Destroy is called when the object is destroyed
    public void OnDestroy()
    {
        Driver.Dispose();
        connections.Dispose();
    }
}
