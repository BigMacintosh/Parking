using UnityEngine;
using System.Collections.Generic;
using Network;


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
        private double countdown;
        private int roundnum;
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
            
        }

        // Update is called once per frame
        private void Update()
        {
            timer++;

            if (active)
            {
                Cursor.visible = true;
            }
            else
            {
                Cursor.visible = false;
            }

            if (Input.GetKey(KeyCode.Escape) && ClientConfig.GameMode != GameMode.AdminMode && !IsServerMode)
            {
                if (!active && timer > 30)
                {
                    escmenu.SetActive(true);
                    active = true;
                    timer = 0;
                }
                else if (timer > 30)
                {
                    escmenu.SetActive(false);
                    Cursor.visible = false;
                    active = false;
                    timer = 0;
                }
            }
            /*if (preroundflag)
            {
                if (countdown > 0)
                {
                    countdown -= Time.deltaTime;
                    hud.eventtext.text = "Round " + roundnum + " starting in " + countdown + " seconds";
                }
                preroundflag = false;
            }
            */
            /*
            if (endroundflag)
            {
                hud.eventtext.text = "Round " + roundnum + " has ended!";
                endroundflag = false;
            }
            */
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
        
        public void OnPreRoundStart(ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort nPlayers, List<ushort> spacesActive)
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
            for (int i=preRoundLength; i>0; i--)
            {
                Invoke("countDownEventText", preRoundLength-i);
                
            }
        }

        public void OnRoundStart(ushort roundNumber)
        {
            // Display message on HUD to say that round is in progress
            hud.eventtext.text = "Round " + roundnum + " has begun!";
            Invoke("clearEventText", 2);
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

        public void countDownEventText()
        {
            hud.eventtext.text = "Round " + roundnum + " starting in " + preroundtimer + " seconds";
            preroundtimer -= 1;
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
