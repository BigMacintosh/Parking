using System;
using UnityEngine;
using System.Collections.Generic;
using Game;
using Network;
using Utils;
using Vehicle;


namespace UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private GameObject escmenu;
        [SerializeField] private GameObject settingsmenu;
        [SerializeField] private AdminMenu adminMenu;

        private Timer _timer;
        
        private HUD _hud;
        public HUD Hud
        {
            get => _hud;
            set => _hud = value;
        }

//        private Timer _countdownTimer;
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


        private ushort _roundLength;
        public UIController()
        {
            _timer = new Timer(0);
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
        }
        
        // Update is called once per frame
        private void Update()
        {
            _timer.Update();
            
            if (Input.GetKey(KeyCode.Escape) && ClientConfig.GameMode != GameMode.AdminMode && !IsServerMode)
            {
                active = !active;
                Cursor.visible = active;
                escmenu.SetActive(active);
            }
            
            if (!(_car is null))
            {
                Hud.Velocity = _car.velocity.magnitude * 3.6f;
            }
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
            _roundLength = roundLength;
            
            // Display countdown on the hud that is preRoundLength seconds long.
            Hud.RoundTextPrefix = "Round " + roundNumber + " starting in ";
            Hud.RoundCountdown = preRoundLength;
            _timer.Stop();
            _timer.SetTime(preRoundLength);
            
            // TODO: Do they need clearing after the timer elapsed?
            _timer.OneSecondPassed += left =>
            {
                Hud.RoundCountdown = (int) left;
            };
            _timer.Elapsed += () =>
            {
                Hud.ClearRoundText();
            };
            _timer.Start();
        }

        public void OnRoundStart(ushort roundNumber, List<ushort> spacesActive)
        {
            // Display message on HUD to say that round is in progress.
            Hud.eventText.text = "Round Started!";
            Hud.RoundTextPrefix = "Round " + roundNumber + " ending in ";
            Hud.RoundCountdown = _roundLength;
            _timer.Stop();
            _timer.SetTime(_roundLength);
            _timer.OneSecondPassed += left => Hud.RoundCountdown = (int) left;
            _timer.Elapsed += () => Hud.ClearRoundText();
            _timer.Start();
        }

        public void OnRoundEnd(ushort roundNumber)
        {
            // Display message on HUD to say that round has ended.
            Hud.eventText.text = "Round " + roundNumber + " has ended!";
        }

        public void OnPlayerCountChange(int nPlayers)
        {
            // Update the player count on the hud
            Hud.NumberOfPlayers = nPlayers;
        }
    }
}
