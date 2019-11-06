using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using NetworkConnection = Unity.Networking.Transport.NetworkConnection;
using UdpCNetworkDriver = Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket,Unity.Networking.Transport.DefaultPipelineStageCollection>;

public class ClientBehaviour : MonoBehaviour
{
    public UdpCNetworkDriver Driver;
    public NetworkConnection Connection;
    public bool Done;

    // private NetworkEndPoint endpoint;
    
    // Start is called before the first frame update
    internal void Start()
    {
        Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        Connection = default(NetworkConnection);

        // TODO: make server endpoint configurable
        var endpoint = NetworkEndPoint.Parse("18.191.231.10", 25565);
        Connection = Driver.Connect(endpoint);
    }

    // Update is called once per frame
    internal void Update()
    {
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
                Debug.Log($"Client: Successfully connected to .");
                
                var value = 1;
                using (var writer = new DataStreamWriter(4, Allocator.Temp))
                {
                    writer.Write(value);
                    Connection.Send(Driver, writer);
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
    
    public void OnDestroy()
    {
        Driver.Dispose();
    }
}
