using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] public Text velocityText;
        [SerializeField] public Text debugText;
        [FormerlySerializedAs("exitbutton")] [SerializeField] public Button exitButton;
        [FormerlySerializedAs("eventtext")] [SerializeField] public Text eventText;
        [FormerlySerializedAs("roundtext")] [SerializeField] public Text roundText;
        [FormerlySerializedAs("playercounttext")] [SerializeField] public Text playerCountText;

        private int _roundCountdown;
        public int RoundCountdown
        {
            get => _roundCountdown;
            set
            {
                _roundCountdown = value;
                UpdateRoundText();
            }
        }
        
        private float _velocity;
        public float Velocity
        {
            get => _velocity;
            set
            {
                _velocity = value;
                UpdateVelocityText();
            }
        }

        private String _networkIP;
        public string NetworkIP
        {
            get => _networkIP;
            set
            {
                _networkIP = value;
                UpdateDebugText();
            }
        }

        private int _numberOfPlayers;

        public int NumberOfPlayers
        {
            get => _numberOfPlayers;
            set
            {
                _numberOfPlayers = value;
                UpdateDebugText();
                UpdatePlayerCountText();
            }
        }

        private String _roundTextPrefix;

        public string RoundTextPrefix
        {
            get => _roundTextPrefix;
            set
            {
                _roundTextPrefix = value;
                UpdateRoundText();
            }
        }

        private void UpdateVelocityText()
        {
            velocityText.text = "Speed: " + _velocity + " !!!! km/h";
        }

        private void UpdateDebugText()
        {
            debugText.text = "Connected to " + _networkIP + "\nNumber of players: "  + NumberOfPlayers;
        }
        
        private void UpdatePlayerCountText()
        {
            playerCountText.text = "Number of players: "  + NumberOfPlayers;
        }

        private void UpdateRoundText()
        {
            roundText.text = RoundTextPrefix + _roundCountdown + " seconds";
        }

        public  void ClearRoundText()
        {
            RoundTextPrefix = "";
            roundText.text = "";
        }
    }
}