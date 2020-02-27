using System;
using System.Collections.Generic;
using Gameplay;
using Network;
using UnityEngine;
using Utils;


namespace Game
{
    static class RoundProperties
    {
        // All times in seconds
        public const ushort FreeroamLength = 10;
        public const ushort PreRoundLength = 5;
        public const ushort RoundLength = 15;

        public const ushort MaxRounds = 5;

        // 0 means no parking spaces would activate, 1 means there would be 1 space per player
        public const float SpacesToPlayersRatio = 0.6f;
    }


    public class RoundManager
    {
        public bool GameInProgress { get; private set; }
        private World world;
        private ushort roundNumber = 0;

        // Timer to countdown to the start of the round.
        private Timer roundTimer;
        private ServerParkingSpaceManager spaceManager;
        private System.Random random;

        // Spawn all the players that have connected. Allow free-roam for the players. Disallow new connections.
        public event GameStartDelegate GameStartEvent;

        // Start a countdown until the beginning of a new round. Provides all the initial info for a round.
        public event PreRoundStartDelegate PreRoundStartEvent;

        // Immediately start a round.
        public event RoundStartDelegate RoundStartEvent;

        // Immediately end a around.
        public event RoundEndDelegate RoundEndEvent;

        public event EliminatePlayersDelegate EliminatePlayersEvent;

        public event GameEndDelegate GameEndEvent;

        public RoundManager(World world, ServerParkingSpaceManager spaceManager)
        {
            this.world = world;
            this.spaceManager = spaceManager;
            this.random = new System.Random();
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
            if (GameInProgress) return;
            NotifyGameStart(RoundProperties.FreeroamLength);
            StartFreeroam();
            GameInProgress = true;
        }

        public void StartFreeroam()
        {
            // Start timer to for PreRoundCountdown 
            roundTimer = new Timer(RoundProperties.FreeroamLength);

            // Add StartRoundEvent to timer observers.
            roundTimer.Elapsed += StartPreRound;
            roundTimer.Start();
        }

        public void StartPreRound()
        {
            // Send pre round warning.
            NotifyPreRoundStart();

            // Start timer to for PreRoundCountdown 
            roundTimer = new Timer(RoundProperties.PreRoundLength);

            // Add StartRoundEvent to timer observers.
            roundTimer.Elapsed += StartRoundEvent;
            roundTimer.Start();
        }

        private void StartRoundEvent()
        {
            int numberOfSpaces = 1;
            // If not the last round
            if (roundNumber < RoundProperties.MaxRounds - 1)
            {
                numberOfSpaces = (int) Math.Ceiling(world.GetNumPlayers() * RoundProperties.SpacesToPlayersRatio);
            }

            Vector2 spacesAround = new Vector2(random.Next(-200, 201), random.Next(-200, 201));
            List<ushort> activeSpaces = spaceManager.GetNearestSpaces(spacesAround, numberOfSpaces);
            Debug.Log(
                $"Round {roundNumber}, {numberOfSpaces} spaces from point ({spacesAround.x}, {spacesAround.y}): {String.Join(", ", activeSpaces)}.");

            NotifyRoundStart(activeSpaces);

            roundTimer = new Timer(RoundProperties.RoundLength);
            roundTimer.Elapsed += EndRoundEvent;
            roundTimer.Start();
        }

        private void EndRoundEvent()
        {
            NotifyRoundEnd();
            roundNumber++;
            if (roundNumber < RoundProperties.MaxRounds && world.GetNumPlayers() > 1)
            {
                var eliminatedPlayers = GetPlayersEliminated();
                NotifyEliminatePlayers(eliminatedPlayers);
                StartPreRound();
            }
            else
            {
                GameEndEvent?.Invoke();
            }
        }

        private List<int> GetPlayersEliminated()
        {
            // Get players in the game
            List<int> playersInGame = world.GetPlayers();
            Dictionary<int, ParkingSpace> playersInSpace = spaceManager.parkingSpacesByPlayerID;

            List<int> eliminatedPlayers = new List<int>();

            foreach (var playerID in playersInGame)
            {
                if (!playersInSpace.ContainsKey(playerID))
                {
                    eliminatedPlayers.Add(playerID);
                }
            }

            return eliminatedPlayers;
        }

        private void NotifyGameStart(ushort freeRoamLength)
        {
            GameStartEvent?.Invoke(freeRoamLength, world.GetNumPlayers());
        }

        private void NotifyPreRoundStart()
        {
            PreRoundStartEvent?.Invoke(roundNumber, RoundProperties.PreRoundLength, RoundProperties.RoundLength,
                world.GetNumPlayers());
        }

        private void NotifyRoundStart(List<ushort> spacesActive)
        {
            RoundStartEvent?.Invoke(roundNumber, spacesActive);
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