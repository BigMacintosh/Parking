using System;
using System.Collections.Generic;
using SceneUtilities;
using UnityEngine;
using UI;

namespace Vehicle
{
    public class Vehicle : MonoBehaviour
    {
        public VehicleProperties vehicleProperties;
        private VehicleDriver driver;

        public List<DriveWheel> driveWheels;


        public void SetControllable()
        {
            // Add DriveController to car
            driver  = gameObject.AddComponent<VehicleDriver>();

            driver.accel = 20;
            driver.maxSpeed = 50;
            driver.driveWheels = driveWheels;
            driver.steer = 4000;
            driver.driftFactor = 0.75f;

            // Set camera to follow car
            var camera = FindObjectOfType<CameraFollowController>();
            camera.ObjectToFollow = transform;

            var minimap = FindObjectOfType<MinimapController>();
            minimap.ObjectToFollow = transform;
            
            // Set car object in HUD.
            var hud = FindObjectOfType<HUD>();
            hud.Car = GetComponent<Rigidbody>();
        }
    }
    
    // Stores properties for each type of car
    // Allows drive controller to change based on different properties.
    [Serializable]
    public class VehicleProperties
    {
        // Scale from 1 - 10
        [field: SerializeField] public int SpeedRating { get; private set; }
        [field: SerializeField] public int SteeringRating { get; private set; }
        [field: SerializeField] public int WeightRating { get; private set; }
    }
}