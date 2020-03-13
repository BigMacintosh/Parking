using System;
using System.Collections.Generic;
using Game.Core.Camera;
using Game.Entity;
using UI;
using UI.Minimap;
using UnityEngine;

namespace Game.Core.Driving {
    [System.Serializable]
    public class WheelTransformPair {
        public WheelCollider wheel;
        public Transform     graphics;
    }

    public class Vehicle : MonoBehaviour {
        // Public Fields
        public CarType                  carType;
        public List<WheelTransformPair> driveWheels;
        public List<WheelTransformPair> otherWheels;
        public VehicleProperties        vehicleProperties;
        public LayerMask                collisionMask;

        [SerializeField] private MeshRenderer bodyRenderer;

        // Private Fields
        private Color         colourToSet;
        private UIController  uiController;
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
            driver               = gameObject.AddComponent<VehicleDriver>();
            driver.driveWheels   = driveWheels;
            driver.otherWheels   = otherWheels;
            driver.maxSteerAngle = 30f;
            driver.motorForce    = 1000f;
            driver.roadMult      = 3f;
            driver.mask          = collisionMask;
            driver.setAcceptInput(true);

            // Set camera to follow car
            var camera = FindObjectOfType<CameraFollowController>();
            camera.ObjectToFollow = transform;

            var minimap = FindObjectOfType<MinimapScroller>();
            minimap.ObjectToFollow = transform;

            //Link with UIController
            uiController         = FindObjectOfType<UIController>();
            uiController.Vehicle = this;
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