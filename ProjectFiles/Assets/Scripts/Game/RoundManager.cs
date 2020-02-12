using System.Collections.Generic;
using Network;
using UnityEngine;
using Utils;

namespace Game
{
    static class RoundTimings
    {
        // All times in seconds
        public const ushort FreeroamLength = 10;
        public const ushort PreRoundLength = 10;
        public const ushort RoundLength = 45;
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

        // Spawn all the players that have connected. Allow free-roam for the players. Disallow new connections.
        public event GameStartDelegate GameStartEvent;

        // Start a countdown until the beginning of a new round.
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
            NotifyGameStart();
            preRoundLength = RoundTimings.PreRoundLength;
            roundLength = RoundTimings.RoundLength;
            freeroamLength = RoundTimings.FreeroamLength;
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