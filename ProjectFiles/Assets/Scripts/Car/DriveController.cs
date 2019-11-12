using System;
using System.Collections.Generic;
using UnityEngine;

namespace Car
{
    public class DriveController : MonoBehaviour
    {
        [SerializeField] private GameObject car;
        public bool isControllable { get; set; }
        [SerializeField] private List<Axle> axles;
        [SerializeField] private float maxMotorTorque;
        [SerializeField] private float maxSteeringAngle;
        
        private Rigidbody rb;
        private float v;
        
        private void Start()
        {
            rb = car.GetComponent<Rigidbody>();
            v = rb.velocity.magnitude * 3.6f; // km/h
        }

        private void FixedUpdate()
        {
            if (isControllable)
            {
                var torque = Input.GetAxis("Vertical") * maxMotorTorque;
                var steering = (Input.GetAxis("Horizontal") * maxSteeringAngle) / TurnMultiplier(v);

                foreach (var axle in axles)
                {
                    if (axle.Steering)
                    {
                        axle.LeftWheel.steerAngle = steering;
                        axle.RightWheel.steerAngle = steering;
                    }

                    if (axle.Motor)
                    {
                        axle.LeftWheel.motorTorque = torque;
                        axle.RightWheel.motorTorque = torque;
                    }
                }
            }
        
        }

        private float TurnMultiplier(float v)
        {
            if (v > 20)
            {
                return 4;
            }
            if (v > 40)
            {
                return 5;
            }
            if (v > 80)
            {
                return 7;
            }
            if (v > 200)
            {
                return 10;
            }
            if (v > 400)
            {
                return 20;
            }
            return 1;
        }

    }

    [Serializable]
    public class Axle
    {
        [field: SerializeField] public WheelCollider LeftWheel { get; private set; }
        [field: SerializeField] public WheelCollider RightWheel { get; private set; }
        [field: SerializeField] public bool Motor { get; private set; } //Does this wheel provide torque
        [field: SerializeField] public bool Steering { get; private set; } //Does this wheel provide steering
    }
}