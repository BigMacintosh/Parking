using System;
using Network;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI {
    public class HUD : MonoBehaviour {
        // Public Fields
        // TODO: sort out the accessors here, we don't want public fields if possible
        public bool HasParkingSpace {
            get => _hasParkingSpace;
            set {
                _hasParkingSpace       = value;
                parkingIcon.color      = HasParkingSpace ? goodThingsGreen : badThingsRed;
                parkingSpaceText.color = HasParkingSpace ? goodThingsGreen : badThingsRed;
            }
        }

        public Text ParkingSpaceText {
            get => parkingSpaceText;
            set => parkingSpaceText = value;
        }

        public int RoundCountdown {
            get => _roundCountdown;
            set {
                _roundCountdown = value;
                UpdateRoundText();
            }
        }

        public string NetworkIP {
            get => _networkIP;
            set {
                _networkIP = value;
                UpdateDebugText();
            }
        }

        public int NumberOfPlayers {
            get => _numberOfPlayers;
            set {
                _numberOfPlayers = value;
                UpdateDebugText();
            }
        }

        public string RoundTextPrefix {
            get => _roundTextPrefix;
            set {
                _roundTextPrefix = value;
                UpdateRoundText();
            }
        }

        // Serializable Fields
        [SerializeField] private Image connectionIcon;
        [SerializeField] private Text  debugText;

        [FormerlySerializedAs("eventtext")] [SerializeField]
        public Text eventText;

        [FormerlySerializedAs("exitbutton")] [SerializeField]
        public Button exitButton;

        [SerializeField] private Image parkingIcon;
        [SerializeField] private Text  parkingSpaceText;

        [FormerlySerializedAs("playercounttext")] [SerializeField]
        private Text playerCountText;

        [FormerlySerializedAs("roundtext")] [SerializeField]
        private Text roundText;

        // Private Fields
        private readonly Color badThingsRed    = new Color(244f / 255f, 67f  / 255f, 54f / 255f);
        private readonly Color goodThingsGreen = new Color(139f / 255f, 195f / 255f, 74f / 255f);

        private bool   _hasParkingSpace;
        private string _networkIP;
        private int    _numberOfPlayers;
        private int    _roundCountdown;
        private string _roundTextPrefix;

        // Get rid of all placeholders when 
        private void Awake() {
//            HasParkingSpace = false;
            ClearRoundText();
            ParkingSpaceText.text = "";
            NetworkIP = "";
            eventText.text = "";
        }

        private void UpdateDebugText() {
            if (_networkIP == null || _networkIP == "Standalone" || _networkIP.Contains("Disconnected")) {
                debugText.color      = badThingsRed;
                connectionIcon.color = badThingsRed;
            } else {
                debugText.color      = goodThingsGreen;
                connectionIcon.color = goodThingsGreen;
            }

            if (ClientConfig.GameMode == GameMode.AdminMode) {
                debugText.text = $"{_networkIP} ({NumberOfPlayers} player{(NumberOfPlayers == 1 ? "" : "s")}) ";
            }

            debugText.text = $"{_networkIP}";
        }

        private void UpdateRoundText() {
            roundText.text = RoundTextPrefix + _roundCountdown + " seconds";
        }

        public void ClearRoundText() {
            RoundTextPrefix = "";
            roundText.text  = "";
        }
    }
}