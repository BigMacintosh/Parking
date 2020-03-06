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
        public LayerMask collisionMask;

        // Private Fields
        private UIController  uicontroller;
        private VehicleDriver driver;


        public void SetControllable() {
            // Add DriveController to car
            driver = gameObject.AddComponent<VehicleDriver>();

            driver.accel       = 20;
            driver.maxSpeed    = 50;
            driver.driveWheels = driveWheels;
            driver.maxSteer    = 30;
            driver.driftFactor = 3f;
            driver.mask        = collisionMask;
            driver.setAcceptInput(true);

            // Set camera to follow car
            var camera = FindObjectOfType<CameraFollowController>();
            camera.ObjectToFollow = transform;

            var minimap = FindObjectOfType<MinimapScroller>();
            minimap.ObjectToFollow = transform;

            //Link with UIController
            uicontroller         = FindObjectOfType<UIController>();
            uicontroller.Vehicle = this;
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