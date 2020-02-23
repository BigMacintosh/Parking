﻿using UnityEngine;
using System.Collections.Generic;


namespace UI
{
    public class UIController : MonoBehaviour
    {
        //  [SerializeField] private GameObject car;
        [SerializeField] private GameObject escmenu;
        [SerializeField] private GameObject settingsmenu;
        private HUD hud;

        private int timer;
        private Rigidbody rb;
        private float v;
        private bool active;

        private void Start()
        {
            Cursor.visible = false;
            escmenu.SetActive(false);
            settingsmenu.SetActive(false);
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

            if (Input.GetKey(KeyCode.Escape))
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

        }

        public void OnPreRoundStart(ushort roundNumber, ushort preRoundLength, ushort roundLength, ushort nPlayers, List<ushort> spacesActive)
        {
            // Display countdown on the hud that is preRoundLength seconds long
        }

        public void OnRoundStart(ushort roundNumber)
        {
            // Display message on HUD to say that round is in progress
            
        }

        public void OnRoundEnd(ushort roundNumber)
        {
            // Display message on HUD to say that round has ended
        }

        public void OnPlayerCountChange(ushort nPlayers)
        {
            // Update the player count on the hud
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
