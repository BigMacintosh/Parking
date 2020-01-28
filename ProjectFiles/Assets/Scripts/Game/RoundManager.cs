using System;
using System.Collections.Generic;
using System.Timers;

namespace Game
{

    static class RoundTimings
    {
        // All times in seconds
        public const int PreRoundLength = 5; 
        public const int RoundLength = 45;
    }
    
    public class RoundManager
    {
        private List<IRoundObserver> observers;
        private World world;
        private int roundNumber = 0;
        
        // Timer to countdown to the start of the round.
        private Timer roundTimer;
        private int preRoundLength;
        private int roundLength;
        private int nPlayers;

        public RoundManager(World world)
        {
            observers = new List<IRoundObserver>();
            this.world = world;
        }
        
        public void StartGame()
        {
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



        // IRoundObserver required functions.
        public void Subscribe(IRoundObserver roundObserver)
        {
            observers.Add(roundObserver);
        }

        private void NotifyPreRoundStart(int preRoundLength, int roundLength, int nPlayers, List<byte> spacesActive)
        {
            foreach (var observer in observers)
            {
                observer.OnPreRoundStart(roundNumber, preRoundLength, roundLength, nPlayers, spacesActive);
            }
        }

        private void NotifyRoundStart()
        {
            foreach (var observer in observers)
            {
                observer.OnRoundStart(roundNumber);
            }
        }

        private void NotifyRoundEnd()
        {
            foreach (var observer in observers)
            {
                observer.OnRoundEnd(roundNumber);
            }
        }

        private void NotifyEliminatePlayers(List<int> eliminatedPlayers)
        {
            foreach (var observer in observers)
            {
                observer.OnEliminatePlayers(roundNumber, eliminatedPlayers);
            }
        }
    }

    public interface IRoundObserver
    {
        // All Players connected, round starting in 5 seconds? Start countdown?
        // All clients know to setup the game.
        void OnPreRoundStart(int roundNumber, int preRoundLength, int roundLength, int nPlayers, List<byte> spacesActive);
        
        // Round actually starts. All clients start when instructed.
        void OnRoundStart(int roundNumber);

        // Ensure all clients round timers stay in sync. Sent every 10/20 seconds maybe?
//        void NotifyTimerSync();
        
        // Pre-warn all clients round is about to end. Start countdown?
//        void NotifyPreRoundEnd();
        
        // Round has actually ended.
        void OnRoundEnd(int roundNumber);
        
        // Elimate players.
        void OnEliminatePlayers(int roundNumber, List<int> eliminatedPlayers);
    }
}