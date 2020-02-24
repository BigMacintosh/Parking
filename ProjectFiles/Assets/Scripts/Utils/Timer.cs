using Game;
using UnityEngine;

namespace Utils
{
    public delegate void TimerOverDelegate();
    public delegate void TimerOneSecondPassedDelegate();
    
    public class Timer
    {
        public event TimerOverDelegate Elapsed;
        public event TimerOneSecondPassedDelegate OneSecondPassed;
        private float timeLeft;
        private float length;
        private bool repeat;

        private float deltaSinceOneSecondPassed;
        
        public bool Set { get; private set; }

        public Timer(float length)
        {
            timeLeft = length;
            deltaSinceOneSecondPassed = 0;
        }
        public Timer(float length, bool repeat) : this(length)
        {
            this.repeat = repeat;
        }


        public void Update()
        {
            var delta = Time.deltaTime;
            timeLeft -= delta;
            deltaSinceOneSecondPassed += delta;
            if (timeLeft < 0 & Set)
            {
                Set = false;
                Elapsed?.Invoke();
            } else if (deltaSinceOneSecondPassed >= 1)
            {
                OneSecondPassed?.Invoke();
                deltaSinceOneSecondPassed = 0;
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