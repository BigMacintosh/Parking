﻿using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Text velocityText;
        [SerializeField] private Text debugText;
        [SerializeField] public Button exitButton;
        [SerializeField] public Text eventText;
        [SerializeField] public Text roundText;
        [SerializeField] public Text playerCountText;
        private float v;
        private String ip;

        public Rigidbody Car { private get; set; }
        public String NetworkIP { private get; set; }
        public int playernum { private get; set; }

        // Update is called once per frame
        private void Update()
        {
            if (!(Car is null))
            {
                v = (float)Math.Round(Car.velocity.magnitude * 3.6f, 0);
                velocityText.text = "Speed: " + v + " km/h";
                
            }
            if (!(NetworkIP is null))
            {
                ip = NetworkIP;
                debugText.text = "Connected to " + ip + "\nNumber of players: "  + playernum;
            }
        }
    }
}