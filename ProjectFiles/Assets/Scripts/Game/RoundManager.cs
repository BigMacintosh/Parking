using System;
using System.Collections.Generic;
using System.Timers;

namespace Game
{
    public class RoundManager
    {
        private List<IRoundObserver> observers;
        private World world;
        private int roundNumber = 0;
        
        // Timers Galore, Keep track of the game
        private Timer roundTimer;
//        private Timer roundTimer;
//        private Timer roundTimer;
//        private Timer roundTimer;
        
        RoundManager(World world)
        {
            observers = new List<IRoundObserver>();
            this.world = world;
        }

        void StartGame()
        {
            // Send Pre round start notification
            
            // Start timer for 5 seconds
        }

        void NewRound()
        {
            roundNumber += 1;
        }
        
        void subscribe(IRoundObserver roundObserver)
        {
            observers.Add(roundObserver);
        }

    }

    public interface IRoundObserver
    {
        // All Players connected, round starting in 5 seconds? Start countdown?
        // All clients know to setup the game.
        void NotifyPreRoundStart();
        
        // Round actually starts. All clients start when instructed.
        void NotifyRoundStart();

        // Ensure all clients round timers stay in sync. Sent every 10/20 seconds maybe?
        void NotifyTimerSync();
        
        // Pre-warn all clients round is about to end. Start countdown?
        void NotifyPreRoundEnd();
        
        // Round has actually ended. Eliminates the players who weren't parked.
        void NotifyRoundEnd(List<int> eliminatedPlayers);
    }
}