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

        private HUD hud;
        private int timer;
        private Rigidbody rb;
        private float v;
        private bool active;
        public bool IsServerMode { get; set; }
        private int roundnum;
        private int roundduration;

        public Vehicle.Vehicle vehicle { get; set; }

        private bool preroundflag = false;
        private bool inroundflag = false;
        private bool endroundflag = false;

        public int preroundtimer;

        private void Start()
        {
            Cursor.visible = false;
            escmenu.SetActive(false);
            settingsmenu.SetActive(false);
            adminMenu.SetServerMode(IsServerMode);
            active = false;
            timer = 0;
            hud = FindObjectOfType<HUD>();
            //hud.Car = vehicle.GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        private void Update()
        {
            timer++;

            if (active)
            {
                Cursor.visible = true;
                //svehicle.getDriver().setAcceptInput(false);
            }
            else
            {
                Cursor.visible = false;
                vehicle.getDriver().setAcceptInput(true);
            }

            if (Input.GetKey(KeyCode.Escape) && ClientConfig.GameMode != GameMode.AdminMode && !IsServerMode)
            {
                if (!active && timer > 30)
                {
                    escmenu.SetActive(true);
                    vehicle.getDriver().setAcceptInput(false);
                    active = true;
                    timer = 0;
                }
                else if (timer > 30)
                {
                    escmenu.SetActive(false);
                    vehicle.getDriver().setAcceptInput(true);
                    if (ClientConfig.GameMode != GameMode.AdminMode)
                    {
                        Cursor.visible = false;
                    }
                    active = false;
                    timer = 0;
                }
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
            hud.eventtext.text = "Round " + roundNumber + " starting in " + 10 + " seconds";
            preroundtimer = preRoundLength;
            roundduration = roundLength;
            for (int i=preRoundLength; i>0; i--)
            {
                Invoke("countDownRoundStart", preRoundLength - i);
            }
        }

        public void OnRoundStart(ushort roundNumber, List<ushort> spacesActive)
        {
            // Display message on HUD to say that round is in progress
            hud.eventtext.text = "Round " + roundnum + " has begun!";
            Invoke("clearEventText", 2);
            for (int i = roundduration; i > 0; i--)
            {
                if(i < 11)
                {
                    roundduration = i;
                    Invoke("countDownRoundEnd", 10 - i);
                }
            }
            hud.roundtext.text = "Round " + roundNumber;
        }

        public void OnRoundEnd(ushort roundNumber)
        {
            // Display message on HUD to say that round has ended
            //roundnum = roundNumber;
            hud.eventtext.text = "Round " + roundnum + " has ended!";
            //endroundflag = true;
        }

        public void OnPlayerCountChange(ushort nPlayers)
        {
            // Update the player count on the hud
            hud.playernum = nPlayers;
            hud.playercounttext.text = nPlayers + " players remaining";
        }

        public void countDownRoundStart()
        {
            hud.eventtext.text = "Round " + roundnum + " starting in " + preroundtimer + " seconds";
            preroundtimer -= 1;
        }

        public void countDownRoundEnd()
        {
            hud.eventtext.text = "Round " + roundnum + " ending in " + roundduration + " seconds";
        }

        public void clearEventText()
        {
            hud.eventtext.text = "";
        }
        
        public HUD getHUD()
        {
            if (hud is null)
            {
                hud = FindObjectOfType<HUD>();
            }
            return hud;
        }
    }
}
