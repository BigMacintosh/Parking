using System.Collections.Generic;

using Network;
using UnityEngine;
using Object = System.Object;

namespace Game
{
    static class RoundTimings
    {
        // All times in seconds
        public const ushort PreRoundLength = 10;
        public const ushort RoundLength = 45;
    }
    public delegate void TimerOverDelegate();
    
    
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
    
    
    public class RoundManager
    {
        public bool Started { get; private set; }
        private World world;
        private ushort roundNumber = 0;
        
        // Timer to countdown to the start of the round.
        private Timer roundTimer;
        private ushort preRoundLength;
        private ushort roundLength;

        public event GameStartDelegate GameStartEvent;
        public event PreRoundStartDelegate PreRoundStartEvent;
        public event RoundStartDelegate RoundStartEvent;
        public event RoundEndDelegate RoundEndEvent;
        public event EliminatePlayersDelegate EliminatePlayersEvent;

        public RoundManager(World world)
        {
            this.world = world;
        }
        
        public void StartGame()
        {
            Started = true;
            NotifyGameStart();
            preRoundLength = RoundTimings.PreRoundLength;
            roundLength = RoundTimings.RoundLength;

            StartPreRound();
        }

        public void Update()
        {
            roundTimer.Update();
        }

        public void StartPreRound()
        {
            List<ushort> activeSpaces = new List<ushort>();
            
            // Send 5 seconds round warning.
            NotifyPreRoundStart(activeSpaces);
            
            // Start timer to for PreRoundCountdown 
            roundTimer = new Timer(preRoundLength);
            
            // Add StartRoundEvent to timer observers.
            roundTimer.Elapsed += StartRoundEvent;
            roundTimer.Start();
        }

        private void StartRoundEvent()
        {
            NotifyRoundStart();
            
            roundTimer = new Timer(roundLength);
            roundTimer.Elapsed += EndRoundEvent;
            roundTimer.Start();
        }

        private void EndRoundEvent()
        {
            NotifyRoundEnd();
            List<ushort> eliminatedPlayers = world.GetPlayersNotInSpace();
            NotifyEliminatePlayers(eliminatedPlayers);
        }

        private void NotifyGameStart()
        {
            GameStartEvent?.Invoke(world.GetNumPlayers());
        }

        private void NotifyPreRoundStart(List<ushort> spacesActive)
        {
            PreRoundStartEvent?.Invoke(roundNumber, preRoundLength, roundLength, world.GetNumPlayers(), spacesActive);
        }

        private void NotifyRoundStart()
        {
            RoundStartEvent?.Invoke(roundNumber);
        }

        private void NotifyRoundEnd()
        {
            RoundEndEvent?.Invoke(roundNumber);
        }

        private void NotifyEliminatePlayers(List<ushort> eliminatedPlayers)
        {
            EliminatePlayersEvent?.Invoke(roundNumber, eliminatedPlayers);
        }
    }
}