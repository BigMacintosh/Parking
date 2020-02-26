using System;
using UnityEngine;
using System.Collections.Generic;
using Game;
using Network;
using UnityEditor;
using Utils;
using Vehicle;


namespace UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private GameObject escmenu;
        [SerializeField] private GameObject settingsmenu;
        [SerializeField] private AdminMenu adminMenu;

        private Timer timer;
        
        private HUD _hud;
        public HUD Hud
        {
            get => _hud;
            set => _hud = value;
        }
        
        private bool active;
        public bool IsServerMode { get; set; }

        private Vehicle.Vehicle _vehicle;

        public Vehicle.Vehicle Vehicle
        {
            get => _vehicle;
            set
            {
                _vehicle = value;
                _car = _vehicle.GetComponent<Rigidbody>();
            }
        }
        private Rigidbody _car;


        private ushort roundLength;
        public UIController()
        {
            timer = new Timer(0);
            active = ClientConfig.GameMode == GameMode.AdminMode;
        }

        private void Awake()
        {
            Hud = FindObjectOfType<HUD>();
        }

        private void Start()
        {
            Cursor.visible = active;
            escmenu.SetActive(false);
            settingsmenu.SetActive(false);
            adminMenu.SetServerMode(IsServerMode);
            
            _hud.HasParkingSpace = false;
            _hud.ParkingSpaceText.text = "No active spaces.";
        }
        
        // Update is called once per frame
        private void Update()
        {
            timer.Update();
            
            if (Input.GetKey(KeyCode.Escape) && ClientConfig.GameMode != GameMode.AdminMode && !IsServerMode)
            {
                active = !active;
                Cursor.visible = active;
                escmenu.SetActive(active);
            }
            
//            if (!(_car is null))
//            {
//                Hud.Velocity = _car.velocity.magnitude * 3.6f;
//            }
        }

        public void SubscribeTriggerGameStartEvent(TriggerGameStartDelegate handler)
        {
            adminMenu.TriggerGameStartEvent += handler;
        }

        public void OnGameStart(ushort freeRoamLength, ushort nPlayers)
        {
            if (ClientConfig.GameMode == GameMode.AdminMode || IsServerMode)
            {
                adminMenu.OnGameStart();
            }
        }
        
        public void OnPreRoundStart(ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort nPlayers)
        {
            // Store the round length to be used later.
            this.roundLength = roundLength;
            
            // Display countdown on the hud that is preRoundLength seconds long.
            Hud.RoundTextPrefix = "Round " + roundNumber + " starting in ";
            Hud.RoundCountdown = preRoundLength;
            
            // Create a new timer otherwise we will subscribe the same method many many times over a whole game.
            timer = new Timer(1, preRoundLength);

            // TODO: Do they need clearing after the timer elapsed?
            timer.Tick += left => Hud.RoundCountdown = left;
            timer.Elapsed += () => Hud.ClearRoundText();
            
            Hud.ParkingSpaceText.text = "No active spaces";
            
            timer.Start();
        }

        public void OnRoundStart(ushort roundNumber, List<ushort> spacesActive)
        {
            // Display message on HUD to say that round is in progress.
            Hud.eventText.text = "Round Started!";
            Hud.RoundTextPrefix = "Round " + roundNumber + " ending in ";
            Hud.RoundCountdown = roundLength;
            
            // Create a new timer otherwise we will subscribe the same method many many times over a whole game.
            timer = new Timer(1, roundLength);
            timer.Tick += left => Hud.RoundCountdown = left;
            timer.Elapsed += () => Hud.ClearRoundText();
            
            Hud.ParkingSpaceText.text = $"{ spacesActive.Count } active spaces";
            
            timer.Start();
        }

        public void OnRoundEnd(ushort roundNumber)
        {
            // Display message on HUD to say that round has ended.
            Hud.eventText.text = "Round " + roundNumber + " has ended!";
            
            Hud.HasParkingSpace = false;
            Hud.ParkingSpaceText.text = "No active spaces";
        }

        public void OnPlayerCountChange(ushort nPlayers)
        {
            // Update the player count on the hud
            Hud.NumberOfPlayers = nPlayers;
        }

        public void OnSpaceStateChange(SpaceState state, ushort spaceID)
        {
            switch (state)
            {
                case SpaceState.EmptyGained:
                    Hud.ParkingSpaceText.text = "You claimed an empty space!";
                    Hud.HasParkingSpace = true;
                    break;
                case SpaceState.StolenGained:
                    Hud.ParkingSpaceText.text = "You stole someone's space!";
                    Hud.HasParkingSpace = true;
                    break;
                case SpaceState.StolenLost:
                    Hud.ParkingSpaceText.text = "Your space was stolen ...";
                    Hud.HasParkingSpace = false;
                    break;
            }
        }
    }
}
