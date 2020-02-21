using System.Collections.Generic;
using Network;
using UnityEngine;
using Utils;

namespace Game
{
    static class DefaultRoundProperties
    {
        // All times in seconds
        public const ushort FreeroamLength = 10;
        public const ushort PreRoundLength = 10;
        public const ushort RoundLength = 5;
        public const ushort MaxRounds = 5;
    }

    public delegate void TimerOverDelegate();

    public class RoundManager
    {
        public bool GameInProgress { get; private set; }
        private World world;
        private ushort roundNumber = 0;

        // Timer to countdown to the start of the round.
        private Timer roundTimer;
        private ushort freeroamLength;
        private ushort preRoundLength;
        private ushort roundLength;
        private ushort maxRounds;

        // Spawn all the players that have connected. Allow free-roam for the players. Disallow new connections.
        public event GameStartDelegate GameStartEvent;

        // Start a countdown until the beginning of a new round. Provides all the initial info for a round.
        public event PreRoundStartDelegate PreRoundStartEvent;

        // Immediately start a round.
        public event RoundStartDelegate RoundStartEvent;

        // Immediately end a around.
        public event RoundEndDelegate RoundEndEvent;
        
        public event EliminatePlayersDelegate EliminatePlayersEvent;

        public RoundManager(World world)
        {
            this.world = world;
        }

        public void Update()
        {
            if (GameInProgress)
            {
                roundTimer.Update();
            }
        }

        public void StartGame()
        {
            preRoundLength = DefaultRoundProperties.PreRoundLength;
            roundLength = DefaultRoundProperties.RoundLength;
            freeroamLength = DefaultRoundProperties.FreeroamLength;
            maxRounds = DefaultRoundProperties.MaxRounds;
            NotifyGameStart(freeroamLength);
            StartFreeroam();
            GameInProgress = true;
        }

        public void StartFreeroam()
        {
            // Start timer to for PreRoundCountdown 
            roundTimer = new Timer(freeroamLength);

            // Add StartRoundEvent to timer observers.
            roundTimer.Elapsed += StartPreRound;
            roundTimer.Start();
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
//            List<ushort> eliminatedPlayers = world.GetPlayersNotInSpace();
//            NotifyEliminatePlayers(eliminatedPlayers);
            roundNumber++;
            if (roundNumber < maxRounds)
            {
                StartPreRound();
            }
        }

        private void NotifyGameStart(ushort freeRoamLength)
        {
            GameStartEvent?.Invoke(freeRoamLength, world.GetNumPlayers());
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