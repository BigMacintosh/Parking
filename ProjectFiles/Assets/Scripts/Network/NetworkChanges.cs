namespace Network
{

    public enum NetworkWorldEvent
    {
        SpawnPlayer,
    }
    
    public struct NetworkChange
    {
        public NetworkWorldEvent WorldEvent;
        
            
    }
}