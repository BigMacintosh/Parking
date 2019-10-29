using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class DriveController : MonoBehaviour
{
    [SerializeField] private List<Axle> axles;

    [SerializeField] private float maxMotorTorque;
    [SerializeField] private float maxSteeringAngle;
    
    // FixedUpdate is 50Hz
    void FixedUpdate()
    {
        float torque = Input.GetAxis("Vertical") * maxMotorTorque;
        float steering = Input.GetAxis("Horizontal") * maxSteeringAngle;

        foreach (Axle axle in axles)
        {
            if (axle.steering)
            {
                axle.leftWheel.steerAngle = steering;
                axle.rightWheel.steerAngle = steering;
            }

            if (axle.motor)
            {
                axle.leftWheel.motorTorque = torque;
                axle.rightWheel.motorTorque = torque;
            }
        }
    }
}

[System.Serializable]
public class Axle
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; //Does this wheel provide torque
    public bool steering; //Does this wheel provide steering
}
