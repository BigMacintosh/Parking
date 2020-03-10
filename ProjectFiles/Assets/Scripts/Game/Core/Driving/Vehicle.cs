using System;
using System.Collections.Generic;
using Game.Core.Camera;
using UI;
using UI.Minimap;
using UnityEngine;

namespace Game.Core.Driving {
    public class Vehicle : MonoBehaviour {
        // Public Fields
        public List<DriveWheel>  driveWheels;
        public VehicleProperties vehicleProperties;
        public LayerMask         collisionMask;
        
        public bool IsPlayer { get; private set; }

        [SerializeField] private MeshRenderer bodyRenderer;

        // Private Fields
        private Color         colourToSet;
        private VehicleDriver driver;


        public void Start() {
            _setBodyColour(colourToSet);
        }

        public void SetBodyColour(Color colour) {
            colourToSet = colour;
        }

        private void _setBodyColour(Color colour) {
            bodyRenderer.materials[0].SetColor("_BaseColor",     colour);
            bodyRenderer.materials[0].SetColor("Color_EF46C5D1", colour);
        }

        public void SetControllable() {
            // Add DriveController to car
            driver = gameObject.AddComponent<VehicleDriver>();

            driver.accel       = 20;
            driver.maxSpeed    = 50;
            driver.driveWheels = driveWheels;
            driver.maxSteer    = 30;
            driver.driftFactor = 3f;
            driver.mask        = collisionMask;
            driver.SetAcceptInput(true);
        }

        public void SetPlayable() {
            // Set camera to follow car
            var camera = FindObjectOfType<CameraFollowController>();
            camera.ObjectToFollow = transform;

            var minimap = FindObjectOfType<MinimapScroller>();
            minimap.ObjectToFollow = transform;

            //Link with UIController
            var uiController = FindObjectOfType<UIController>();
            uiController.Vehicle = this;

            IsPlayer = true;
        }
    }

    // Stores properties for each type of car
    // Allows drive controller to change based on different properties.
    [Serializable]
    public class VehicleProperties {
        // Scale from 1 - 10
        [field: SerializeField] public int SpeedRating    { get; private set; }
        [field: SerializeField] public int SteeringRating { get; private set; }
        [field: SerializeField] public int WeightRating   { get; private set; }
    }
}
