using Game;
using UnityEngine;
using UnityEngine.Polybrush;

namespace Utils
{
    public delegate void TimerOverDelegate();
    public delegate void TimerTickDelegate(int ticksLeft);
    
    public class Timer
    {
        public event TimerOverDelegate Elapsed;
        public event TimerTickDelegate Tick;
        private float timeLeft;
        private int ticksLeft;
        private float length;
        private int nTicks;
        private bool repeat;
        
        
        public bool Set { get; private set; }

        public Timer(float length)
        {
            SetTime(length);
            Set = false;
        }
        
        public Timer(float length, bool repeat) : this(length)
        {
            this.repeat = repeat;
            
            // Repeat 'infinitely'
            SetTicks(int.MaxValue);
        }

        public Timer(float length, int nTicks) : this(length)
        {
            this.repeat = true;
            
            // Repeat nTicks times.
            SetTicks(nTicks);
        }


        // Funily enough like almost everything that isnt a monobehavior
        // when we give it an Update function, you have to call it.
        // big shock?!?!
        public void Update()
        {
            if (Set)
            {
                var delta = Time.deltaTime;
                timeLeft -= delta;

                if (timeLeft < 0)
                {

                    // Deal with timer tick
                    if (repeat)
                    {
                        ticksLeft -= 1;
                        timeLeft += length;
                        Tick?.Invoke(ticksLeft);
                    }

                    // Deal with timer end.
                    if (nTicks <= 0 && repeat || !repeat)
                    {
                        Set = false;
                        Elapsed?.Invoke();
                    }
                }
            }
        }

        public void Start()
        {
            Set = true;
        }

        public void Stop()
        {
            Set = false;
        }
        
        public void Reset()
        {
            Set = false;
            timeLeft = length;
            ticksLeft = nTicks;
        }
        
        public void SetTime(float length)
        {
            this.length = length;
            timeLeft = length;
        }

        public void SetTicks(int nTicks)
        {
            this.nTicks = nTicks;
            ticksLeft = nTicks;
        }
    }
}