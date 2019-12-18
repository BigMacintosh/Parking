using System;

namespace Game
{
    public delegate void ChickenDelegate(string str);
    
    public class EventExample
    {
        public void DoSomething(string str)
        {
            // run this when an event fires
            Console.WriteLine(str);
        }
    }

    public class Chicken
    {
        public event ChickenDelegate ChickenDidSomething ;

        private EventExample example = new EventExample();

        public Chicken()
        {
            ChickenDidSomething += example.DoSomething;
        }
        
        public void EmitEvent()
        {
            ChickenDidSomething?.Invoke("WOW");
        }
    }
}