using Game;
using UnityEngine;

namespace Utils
{
    public delegate void TimerOverDelegate();
    
    public class Timer
    {
        public event TimerOverDelegate Elapsed;
        private float timeLeft;
        private float length;
        private bool repeat;
        
        public bool Set { get; private set; }

        public Timer(float length)
        {
            timeLeft = length;
        }
        public Timer(float length, bool repeat) : this(length)
        {
            this.repeat = repeat;
        }


        public void Update()
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0 & Set)
            {
                Set = false;
                Elapsed?.Invoke();
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
        }
        
        public void SetTime(float length)
        {
            this.length = length;
        }
    }
}