using System.Collections.Generic;
using Game.Core.Driving;
using Network;
using UI.InGameMenu;
using UnityEngine;
using Utils;

namespace UI {
    public class UIController : MonoBehaviour {
        // Public Fields
        public HUD Hud { get; set; }

        public bool IsServerMode { get; set; }

        public Vehicle Vehicle {
            get => _vehicle;
            set {
                _vehicle = value;
                _car     = _vehicle.GetComponent<Rigidbody>();
            }
        }

        // Serializable Fields
        [SerializeField] private AdminMenu  adminMenu;
        [SerializeField] private GameObject escmenu;
        [SerializeField] private GameObject settingsmenu;

        // Private Fields
        private Rigidbody _car;
        private Vehicle   _vehicle;
        private bool      active;
        private ushort    roundLength;


        private Timer timer;

        public UIController() {
            timer  = new Timer(0);
            active = ClientConfig.GameMode == GameMode.AdminMode;
        }

        private void Awake() {
            Hud = FindObjectOfType<HUD>();
        }

        private void Start() {
            Cursor.visible = active;
            escmenu.SetActive(false);
            settingsmenu.SetActive(false);
            adminMenu.SetServerMode(IsServerMode);

            Hud.HasParkingSpace = false;
            Hud.NumActiveSpaces = 0;
        }

        // Update is called once per frame
        private void Update() {
            timer.Update();

            if (Input.GetKey(KeyCode.Escape) && ClientConfig.GameMode != GameMode.AdminMode && !IsServerMode) {
                active         = !active;
                Cursor.visible = active;
                escmenu.SetActive(active);
            }

//            if (!(_car is null))
//            {
//                Hud.Velocity = _car.velocity.magnitude * 3.6f;
//            }
        }

        public void SubscribeTriggerGameStartEvent(TriggerGameStartDelegate handler) {
            adminMenu.TriggerGameStartEvent += handler;
        }

        public void OnGameStart(ushort freeRoamLength, ushort nPlayers) {
            if (ClientConfig.GameMode == GameMode.AdminMode || IsServerMode) {
                adminMenu.OnGameStart();
            }
        }

        public void OnPreRoundStart(ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort nPlayers) {
            // Store the round length to be used later.
            this.roundLength = roundLength;

            Hud.RoundText = $"Round {roundNumber} starting in {preRoundLength}.";

            // Create a new timer otherwise we will subscribe the same method many many times over a whole game.
            timer = new Timer(1, preRoundLength);

            // TODO: Do they need clearing after the timer elapsed?
            timer.Tick    += left => Hud.RoundText = $"Round {roundNumber} starting in {left}.";
            timer.Elapsed += () => Hud.RoundText   = "";

            Hud.NumActiveSpaces = 0;

            timer.Start();
        }

        public void OnRoundStart(ushort roundNumber, List<ushort> spacesActive) {
            // Display message on HUD to say that round is in progress.
            Hud.EventText = "Round Started!";
            Hud.RoundText = $"Round {roundNumber} ending in {roundLength}";

            // Create a new timer otherwise we will subscribe the same method many many times over a whole game.
            timer         =  new Timer(1, roundLength);
            timer.Tick    += left => Hud.RoundText = $"Round {roundNumber} ending in {left}";
            timer.Elapsed += () => Hud.RoundText   = "";

            Hud.NumActiveSpaces = spacesActive.Count;

            timer.Start();
        }

        public void OnRoundEnd(ushort roundNumber) {
            // Display message on HUD to say that round has ended.
            Hud.EventText = "Round " + roundNumber + " has ended!";

            Hud.HasParkingSpace = false;
            Hud.NumActiveSpaces = 0;
        }

        public void OnPlayerCountChange(ushort nPlayers) {
            // Update the player count on the hud
            Hud.NumberOfPlayers = nPlayers;
        }

        public void OnSpaceStateChange(SpaceState state, ushort spaceID) {
            switch (state) {
                case SpaceState.EmptyGained:
                    Hud.EventText       = "You claimed an empty space!";
                    Hud.HasParkingSpace = true;
                    break;
                case SpaceState.StolenGained:
                    Hud.EventText       = "You stole someone's space!";
                    Hud.HasParkingSpace = true;
                    break;
                case SpaceState.StolenLost:
                    Hud.HasParkingSpace = false;
                    break;
            }
        }

        public void OnGameEnd(List<int> winners) {
            foreach (var player in winners) {
                if (player == ClientConfig.PlayerID) {
                    Hud.EventText = "YOU WON !!!";
                    return;
                }
            }

            Hud.EventText = "You lost ...";
        }

        public void OnEliminatePlayers(ushort roundNumber, List<int> eliminated) {
            foreach (var player in eliminated) {
                if (player == ClientConfig.PlayerID) {
                    Hud.EventText = "You lost ...";
                    return;
                }
            }
        }
    }
}