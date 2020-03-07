using UnityEngine;

namespace Utils {
    public delegate void TimerOverDelegate();

    public delegate void TimerTickDelegate(int ticksLeft);

    public class Timer {
        // Delegates
        public event TimerOverDelegate Elapsed;
        public event TimerTickDelegate Tick;

        // Public Fields
        public bool Set { get; private set; }

        // Private Fields
        private int   nTicks;
        private int   ticksLeft;
        private float length;
        private float timeLeft;

        private readonly bool repeat;

        public Timer(float length) {
            SetTime(length);
            
            Set = false;
        }

        public Timer(float tickLength, bool repeat) : this(tickLength) {
            this.repeat = repeat;

            // Repeat 'infinitely'
            SetTicks(int.MaxValue);
        }

        public Timer(float tickLength, int nTicks) : this(tickLength) {
            repeat = true;

            // Repeat nTicks times.
            SetTicks(nTicks);
        }
        
        public void Update() {
            if (Set) {
                var delta = Time.deltaTime;
                timeLeft -= delta;

                if (timeLeft < 0) {
                    // Deal with timer tick
                    if (repeat) {
                        ticksLeft -= 1;
                        timeLeft  += length;
                        Tick?.Invoke(ticksLeft);
                    }

                    // Deal with timer end.
                    if (ticksLeft <= 0 && repeat || !repeat) {
                        Set = false;
                        Elapsed?.Invoke();
                    }
                }
            }
        }

        public void Start() {
            Set = true;
        }

        public void Stop() {
            Set = false;
        }

        public void Reset() {
            Set       = false;
            timeLeft  = length;
            ticksLeft = nTicks;
        }

        public void SetTime(float length) {
            this.length = length;
            timeLeft    = length;
        }

        public void SetTicks(int nTicks) {
            this.nTicks = nTicks;
            ticksLeft   = nTicks;
        }
    }
}