using System;
using System.Collections.Generic;
using System.Timers;
using Network;

namespace Game
{
    static class RoundTimings
    {
        // All times in seconds
        public const int PreRoundLength = 10;
        public const int RoundLength = 45;
    }
    
    public class RoundManager
    {
        public bool Started { get; private set; }
        private World world;
        private int roundNumber = 0;
        
        // Timer to countdown to the start of the round.
        private Timer roundTimer;
        private int preRoundLength;
        private int roundLength;
        private int nPlayers;
        
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
            NotifyGameStart(world.GetNumPlayers());
            preRoundLength = RoundTimings.PreRoundLength;
            roundLength = RoundTimings.RoundLength;
            nPlayers = world.GetNumPlayers();

            StartPreRound();
        }

        public void StartPreRound()
        {
            List<byte> activeSpaces = new List<byte>();
            
            // Send 5 seconds round warning.
            NotifyPreRoundStart(preRoundLength, roundLength, nPlayers, activeSpaces);
            
            // Start timer to for PreRoundCountdown 
            roundTimer = new Timer(preRoundLength * 1000);
            // Add StartRoundEvent to timer observers.
            roundTimer.Elapsed += StartRoundEvent;
            roundTimer.Start();
        }

        private void StartRoundEvent(Object source, ElapsedEventArgs e)
        {
            NotifyRoundStart();
            
            roundTimer = new Timer(roundLength * 1000);
            roundTimer.Elapsed += EndRoundEvent;
            roundTimer.Start();
            
        }

        private void EndRoundEvent(Object source, ElapsedEventArgs e)
        {
            NotifyRoundEnd();
            List<int> eliminatedPlayers = world.GetPlayersNotInSpace();
            NotifyEliminatePlayers(eliminatedPlayers);
        }

        private void NotifyGameStart(int nPlayers)
        {
            GameStartEvent?.Invoke(nPlayers);
        }

        private void NotifyPreRoundStart(int preRoundLength, int roundLength, int nPlayers, List<byte> spacesActive)
        {
            PreRoundStartEvent?.Invoke(roundNumber, preRoundLength, roundLength, nPlayers, spacesActive);
        }

        private void NotifyRoundStart()
        {
            RoundStartEvent?.Invoke(roundNumber);
        }

        private void NotifyRoundEnd()
        {
            RoundEndEvent?.Invoke(roundNumber);
        }

        private void NotifyEliminatePlayers(List<int> eliminatedPlayers)
        {
            EliminatePlayersEvent?.Invoke(roundNumber, eliminatedPlayers);
        }
    }
}