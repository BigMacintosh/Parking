using Game;
using UnityEngine;

namespace Utils
{
    public delegate void TimerOverDelegate();
    public delegate void TimerOneSecondPassedDelegate(float timeLeft);
    
    public class Timer
    {
        public event TimerOverDelegate Elapsed;
        public event TimerOneSecondPassedDelegate OneSecondPassed;
        private float timeLeft;
        private float length;
//        private bool repeat;

        private float deltaSinceOneSecondPassed;
        
        public bool Set { get; private set; }

        public Timer(float length)
        {
            SetTime(length);
            Set = false;
        }
        
        // TODO: Repeat does not actually work
//        public Timer(float length, bool repeat) : this(length)
//        {
//            this.repeat = repeat;
//        }


        // Timer DOES NOT update itself !!!
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
//                Debug.Log("Invoked! " + deltaSinceOneSecondPassed + " " + timeLeft);
                OneSecondPassed?.Invoke(timeLeft);
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
            timeLeft = length;
            deltaSinceOneSecondPassed = 0;
        }
    }
}