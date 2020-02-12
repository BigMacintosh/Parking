using Game;
using UnityEngine;

namespace Utils
{
    public class Timer
    {
        public event TimerOverDelegate Elapsed;
        private float timeLeft;
        private bool started;

        public Timer(float length)
        {
            timeLeft = length;
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
    }
}