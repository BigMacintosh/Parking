using Game;
using UnityEngine;

namespace Utils
{
    public class Timer
    {
        public event TimerOverDelegate Elapsed;
        private float timeLeft;
        private bool started;
        private bool repeat;

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
            if (timeLeft < 0 & started)
            {
                started = false;
                Elapsed?.Invoke();
            }
        }

        public void Start()
        {
            started = true;
        }

        public void Stop()
        {
            started = false;
        }
    }
}