using System;
using System.Net;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;


namespace UI
{
    public class HUD : MonoBehaviour
    {
        // TODO: sort out the accessors here, we don't want public fields if possible
        [SerializeField] private Text velocityText;
        [SerializeField] private Text debugText;
        [FormerlySerializedAs("exitbutton")] [SerializeField]
        public Button exitButton;

        [FormerlySerializedAs("eventtext")] [SerializeField]
        public Text eventText;

        [FormerlySerializedAs("roundtext")] [SerializeField]
        private Text roundText;

        [FormerlySerializedAs("playercounttext")] [SerializeField]
        private Text playerCountText;

        
        [SerializeField] private Image connectionIcon;
        [SerializeField] private Image parkingIcon;
        [SerializeField] private Text parkingSpaceText;
        private readonly Color goodThingsGreen = new Color(139f/255f, 195f/255f, 74f/255f);
        private readonly Color badThingsRed = new Color(244f/255f, 67f/255f, 54f/255f);

        private bool _hasParkingSpace;
        public bool HasParkingSpace
        {
            get => _hasParkingSpace;
            set
            {
                _hasParkingSpace = value;
                parkingIcon.color = HasParkingSpace ? goodThingsGreen : badThingsRed;
                parkingSpaceText.color = HasParkingSpace ? goodThingsGreen : badThingsRed;
            }
        }

        public Text ParkingSpaceText
        {
            get => parkingSpaceText;
            set => parkingSpaceText = value;
        }

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

        private short spaceID;

        public short SpaceID
        {
            get => spaceID;
            set { spaceID = value; }
        }

        private void UpdateDebugText()
        {

            if (_networkIP == null || (_networkIP == "Standalone" || _networkIP.Contains("Disconnected")))
            {
                debugText.color = badThingsRed;
                connectionIcon.color = badThingsRed;
            }
            else
            {
                debugText.color = goodThingsGreen;
                connectionIcon.color = goodThingsGreen;
            }
            
            if (ClientConfig.GameMode == GameMode.AdminMode)
            {
                debugText.text = $"{ _networkIP } ({ NumberOfPlayers } player{ (NumberOfPlayers == 1 ? "" : "s") }) ";
            }
            
            debugText.text = $"{_networkIP}";
        }

        private void UpdateRoundText()
        {
            roundText.text = RoundTextPrefix + _roundCountdown + " seconds";
        }

        public void ClearRoundText()
        {
            RoundTextPrefix = "";
            roundText.text = "";
        }
    }
}