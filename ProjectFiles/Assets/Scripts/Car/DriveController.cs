using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class DriveController : MonoBehaviour
{
    public GameObject car;
    private Rigidbody rb;
    private float v;
    public bool isControllable;

    [SerializeField] private List<Axle> axles;

    [SerializeField] private float maxMotorTorque;
    [SerializeField] private float maxSteeringAngle;
    

    private void Start()
    {
        rb = car.GetComponent<Rigidbody>();
        v = rb.velocity.magnitude * 3.6f; // km/h
    }

    // FixedUpdate is 50Hz
    void FixedUpdate()
    {
        if (isControllable)
        {
            float torque = Input.GetAxis("Vertical") * maxMotorTorque;
            float steering = (Input.GetAxis("Horizontal") * maxSteeringAngle) / TurnMultiplier(v);

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

    public float TurnMultiplier(float v)
    {
        if (v > 20)
            return 4;
        else if (v > 40)
            return 5;
        else if (v > 80)
            return 7;
        else if (v > 200)
            return 10;
        else if (v > 400)
            return 20;
        else
            return 1;
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

