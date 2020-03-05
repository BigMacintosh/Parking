using System;
using System.Collections.Generic;
using Game.Core.Parking;
using Game.Entity;
using Network;
using UnityEngine;
using Utils;
using Random = System.Random;

namespace Game.Core.Rounds {
    /// <summary>
    /// Stores key properties for rounds.
    /// </summary>
    internal static class RoundProperties {
        // All times in seconds
        public const ushort FreeroamLength = 30;
        public const ushort PreRoundLength = 5;
        public const ushort RoundLength    = 30;
        public const ushort MaxRounds      = 5;

        // 0 means no parking spaces would activate, 1 means there would be 1 space per player
        public const float SpacesToPlayersRatio = 0.8f;
    }


    /// <summary>
    /// The main component for controlling the game logic.
    ///  - Times the rounds
    ///  - Controls who is eliminated at the end of the round.
    /// </summary>
    public class RoundManager {
        // Private Fields
        private readonly ServerParkingSpaceManager spaceManager;
        private readonly Random                    random;
        private readonly World                     world;

        private ushort roundNumber;
        private Timer  roundTimer; // Timer to countdown to the start of the round.
        private bool   gameInProgress;

        public RoundManager(World world, ServerParkingSpaceManager spaceManager) {
            this.world        = world;
            this.spaceManager = spaceManager;
            random            = new Random();
        }


        /// <summary>
        /// Spawn all the players that have connected. Allow free-roam for the players. Disallow new connections.
        /// </summary>
        public event GameStartDelegate GameStartEvent;

        /// <summary>
        /// Start a countdown until the beginning of a new round. Provides all the initial info for a round.
        /// </summary>
        public event PreRoundStartDelegate PreRoundStartEvent;

        /// <summary>
        /// Immediately start a round.
        /// </summary>
        public event RoundStartDelegate RoundStartEvent;

        /// <summary>
        /// Immediately end a around.
        /// </summary>
        public event RoundEndDelegate RoundEndEvent;

        /// <summary>
        /// To inform components that some players have been eliminated.
        /// </summary>
        public event EliminatePlayersDelegate EliminatePlayersEvent;

        /// <summary>
        /// To inform components that the game has ended.
        /// </summary>
        public event GameEndDelegate GameEndEvent;

        /// <summary>
        /// Required to update the timer.
        /// </summary>
        public void Update() {
            if (gameInProgress) {
                roundTimer.Update();
            }
        }

        /// <summary>
        /// Used to start a game.
        /// </summary>
        public void StartGame() {
            if (gameInProgress) return;
            GameStartEvent?.Invoke(RoundProperties.FreeroamLength, world.GetNumPlayers());
            StartFreeroam();
            gameInProgress = true;
        }


        /// <summary>
        /// Starts the freeroam period at the start of each game.
        /// </summary>
        private void StartFreeroam() {
            // Start timer to for PreRoundCountdown 
            roundTimer = new Timer(RoundProperties.FreeroamLength);

            // Add StartRoundEvent to timer observers.
            roundTimer.Elapsed += StartPreRound;
            roundTimer.Start();
        }

        /// <summary>
        /// Starts the pre-round section. Allows for a timer to countdown into the round.
        /// </summary>
        private void StartPreRound() {
            // Send pre round warning.
            PreRoundStartEvent?.Invoke(roundNumber, RoundProperties.PreRoundLength, RoundProperties.RoundLength,
                                       world.GetNumPlayers());

            // Start timer to for PreRoundCountdown 
            roundTimer = new Timer(RoundProperties.PreRoundLength);

            // Add StartRoundEvent to timer observers.
            roundTimer.Elapsed += StartRoundEvent;
            roundTimer.Start();
        }

        /// <summary>
        /// Starts a round. Generates all the spaces to activate and informs game components.
        /// </summary>
        private void StartRoundEvent() {
            roundNumber++;
            var numberOfSpaces = 1;
            // If not the last round
            if (roundNumber < RoundProperties.MaxRounds - 1) {
                numberOfSpaces = (int) Math.Floor(world.GetNumPlayers() * RoundProperties.SpacesToPlayersRatio);
            }

            var spacesAround = new Vector2(random.Next(-200, 201), random.Next(-200, 201));
            var activeSpaces = spaceManager.GetNearestSpaces(spacesAround, numberOfSpaces);
            Debug.Log(
                $"Round {roundNumber}, {numberOfSpaces} spaces from point ({spacesAround.x}, {spacesAround.y}): {string.Join(", ", activeSpaces)}.");

            RoundStartEvent?.Invoke(roundNumber, activeSpaces);

            roundTimer         =  new Timer(RoundProperties.RoundLength);
            roundTimer.Elapsed += EndRoundEvent;
            roundTimer.Start();
        }

        /// <summary>
        /// Ends a round. Calculates all the players who have been eliminated.
        /// </summary>
        private void EndRoundEvent() {
            Debug.Log($"Round {roundNumber} ended.");

            RoundEndEvent?.Invoke(roundNumber);

            var eliminatedPlayers = GetEliminatedPlayers();
            Debug.Log($"Eliminated Players: {string.Join(", ", eliminatedPlayers)}");

            var nextNumPlayers = world.GetNumPlayers() - eliminatedPlayers.Count;
            Debug.Log($"Next num players {nextNumPlayers}.");

            // Check if the game should end.
            if (roundNumber <= RoundProperties.MaxRounds && nextNumPlayers > 1) {
                Debug.Log("Continuing the game...");
                EliminatePlayersEvent?.Invoke(roundNumber, eliminatedPlayers);
                StartPreRound();
            } else {
                Debug.Log("The game has finished...");
                var winners = GetWinners();
                Debug.Log($"Winners: {string.Join(", ", winners)}");
                GameEndEvent?.Invoke(winners);
            }
        }

        /// <summary>
        /// Calculate which players should be eliminated.
        /// </summary>
        /// <returns>List of playerIDs who have been eliminted</returns>
        private List<int> GetEliminatedPlayers() {
            // Get players in the game
            var playersInGame  = world.GetPlayersInGame();
            var playersInSpace = spaceManager.ParkingSpacesByPlayerID;

            var eliminatedPlayers = new List<int>();

            foreach (var playerID in playersInGame) {
                if (!playersInSpace.ContainsKey(playerID)) {
                    eliminatedPlayers.Add(playerID);
                }
            }

            return eliminatedPlayers;
        }

        /// <summary>
        /// Calculates who has won a game.
        /// </summary>
        /// <returns>List of playerIDs who have won.</returns>
        private List<int> GetWinners() {
            // Get players in the game
            var playersInGame  = world.GetPlayersInGame();
            var playersInSpace = spaceManager.ParkingSpacesByPlayerID;

            var winners = new List<int>();

            foreach (var playerID in playersInGame) {
                if (playersInSpace.ContainsKey(playerID)) {
                    winners.Add(playerID);
                    Debug.Log($"Winners: {playerID}");
                }
            }

            return winners;
        }
    }
}