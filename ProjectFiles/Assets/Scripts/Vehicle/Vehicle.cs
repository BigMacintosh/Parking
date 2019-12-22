using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle
{
    public class Vehicle : MonoBehaviour
    {
        public List<Axle> axles;
        public VehicleProperties vehicleProperties;
        private DriveController driveController;


        public void SetControllable()
        {
            // Add DriveController to car
            //driveController = gameObject.AddComponent<DriveController>();
            gameObject.AddComponent<VehicleDriver>();
            //driveController.Axles = axles;
            
            // Set camera to follow car
            var camera = FindObjectOfType<CameraFollowController>();
            camera.ObjectToFollow = transform;
            
            // Set car object in HUD.
            HUD hud = FindObjectOfType<HUD>();
            hud.Car = GetComponent<Rigidbody>();
        }
    }
    
    // Stores axles
    [Serializable]
    public class Axle
    {
        [field: SerializeField] public WheelCollider LeftWheel { get; private set; }
        [field: SerializeField] public WheelCollider RightWheel { get; private set; }
        [field: SerializeField] public bool Motor { get; private set; } // Does this wheel provide torque
        [field: SerializeField] public bool Steering { get; private set; } // Does this wheel provide steering
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