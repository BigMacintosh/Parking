using System;
using UnityEngine;
using System.Collections.Generic;
using Game;
using Network;
using Vehicle;


namespace UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private GameObject escmenu;
        [SerializeField] private GameObject settingsmenu;
        [SerializeField] private AdminMenu adminMenu;

        private HUD _hud;
        public HUD Hud
        {
            get => _hud;
            set => _hud = value;
        }

        private int timer;
        private Rigidbody _rb;
        private float v;
        private bool active;
        public bool IsServerMode { get; set; }
        private int roundnum;
        private int roundduration;

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
        
        private bool preroundflag = false;
        private bool inroundflag = false;
        private bool endroundflag = false;

        public int preroundtimer;


        public UIController()
        {
            active = ClientConfig.GameMode == GameMode.AdminMode;
            timer = 0;
        }

        private void Awake()
        {
            Debug.Log("1-----------" + Hud);
            Hud = FindObjectOfType<HUD>();
            Debug.Log("2-----------" + Hud);
        }

        private void Start()
        {
            Cursor.visible = false;
            escmenu.SetActive(false);
            settingsmenu.SetActive(false);
            adminMenu.SetServerMode(IsServerMode);
        }

//        private void Start()
//        {
//            Cursor.visible = false;
//            escmenu.SetActive(false);
//            settingsmenu.SetActive(false);
//            adminMenu.SetServerMode(IsServerMode);
//            active = ClientConfig.GameMode == GameMode.AdminMode;
//            timer = 0;
//            Debug.Log("1-----------" + Hud);
//            Hud = FindObjectOfType<HUD>();
//            Debug.Log("2-----------" + Hud);
//        }

        // Update is called once per frame
        private void Update()
        {
            Cursor.visible = active;

            if (Input.GetKey(KeyCode.Escape) && ClientConfig.GameMode != GameMode.AdminMode && !IsServerMode)
            {
                active = !active;
                escmenu.SetActive(active);
                
//                if (!active && timer > 30)
//                {
//                    escmenu.SetActive(true);
//                    // vehicle.getDriver().setAcceptInput(false);
//                    active = true;
//                    timer = 0;
//                }
//                else if (timer > 30)
//                {
//                    escmenu.SetActive(false);
//                    // vehicle.getDriver().setAcceptInput(true);
//                    if (ClientConfig.GameMode != GameMode.AdminMode)
//                    {
//                        Cursor.visible = false;
//                    }
//                    active = false;
//                    timer = 0;
//                }
            }
            
            if (!(_car is null))
            {
                _hud.Velocity = _car.velocity.magnitude * 3.6f;
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
            // Display countdown on the hud that is preRoundLength seconds long
            //countdown = preRoundLength;
            roundnum = roundNumber;
            //preroundflag = true;
            /*for (int i=preRoundLength; i>0; i--)
            {
                Invoke(hud.eventtext.text = "Round " + roundNumber + " starting in " + i + " seconds", (preRoundLength - i));
            }*/
//            _hud.eventText.text = "Round " + roundNumber + " starting in " + 10 + " seconds";
            preroundtimer = preRoundLength;
            roundduration = roundLength;
            for (int i=preRoundLength; i>0; i--)
            {
                Invoke("CountDownRoundStart", preRoundLength - i);
            }
        }

        public void OnRoundStart(ushort roundNumber, List<ushort> spacesActive)
        {
            // Display message on HUD to say that round is in progress
//            _hud.eventText.text = "Round " + roundnum + " has begun!";
            Invoke("ClearEventText", 2);
            for (int i = roundduration; i > 0; i--)
            {
                if(i < 11)
                {
                    roundduration = i;
                    Invoke("CountDownRoundEnd", 10 - i);
                }
            }
//            _hud.roundText.text = "Round " + roundNumber;
        }

        public void OnRoundEnd(ushort roundNumber)
        {
            // Display message on HUD to say that round has ended
            //roundnum = roundNumber;
//            _hud.eventText.text = "Round " + roundnum + " has ended!";
            //endroundflag = true;
        }

        public void OnPlayerCountChange(int nPlayers)
        {
            // Update the player count on the hud
            Hud.NumberOfPlayers = nPlayers;
        }

        public void CountDownRoundStart()
        {
//            _hud.eventText.text = "Round " + roundnum + " starting in " + preroundtimer + " seconds";
            preroundtimer -= 1;
        }

        public void CountDownRoundEnd()
        {
//            _hud.eventText.text = "Round " + roundnum + " ending in " + roundduration + " seconds";
        }

        public void ClearEventText()
        {
            _hud.eventText.text = "";
        }
    }
}
