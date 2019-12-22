using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle
{
    public class Vehicle : MonoBehaviour
    {
        public VehicleProperties vehicleProperties;
        private VehicleDriver driver;


        public void SetControllable()
        {
            // Add DriveController to car
            driver  = gameObject.AddComponent<VehicleDriver>();

            driver.accel = 5;
            driver.maxSpeed = 30;

            // Set camera to follow car
            var camera = FindObjectOfType<CameraFollowController>();
            camera.ObjectToFollow = transform;
            
            // Set car object in HUD.
            HUD hud = FindObjectOfType<HUD>();
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