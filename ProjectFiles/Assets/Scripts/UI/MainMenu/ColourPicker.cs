using System.Collections.Generic;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu {
    public class ColourPicker : MonoBehaviour {
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private RawImage colourView;
        
        private int counter;
        private List<Color> carColours;

        public void Start() {
            carColours = new List<Color> {
                Color.blue,
                Color.cyan,
                Color.green,
                Color.magenta,
                Color.red
            };
            SetCarColour(carColours[0]);
            
            leftButton.enabled = false;
            if (carColours.Count == 1) {
                rightButton.enabled = false;
            }
        }

        public void OnLeftButtonClick() {
            Debug.Log("Left button clicked");
            if (counter == carColours.Count - 1) {
                rightButton.enabled = true;
            }
            
            counter--;
            
            if (counter == 0) {
                leftButton.enabled = false;
            }
            
            SetCarColour(carColours[counter]);
        }

        public void OnRightButtonClick() {
            Debug.Log("Right button clicked");
            if (counter == 0) {
                leftButton.enabled = true;
            }

            counter++;

            if (counter == carColours.Count - 1) {
                rightButton.enabled = false;
            }

            SetCarColour(carColours[counter]);
        }

        public void SetCarColour(Color colour) {
            colourView.color = colour;
            ClientConfig.VehicleColour = carColours[counter];
        }
    }
}