using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleDriver : MonoBehaviour
{
    private Rigidbody body;
    
    //Car Properties

    [SerializeField] private float maxSpeed, accel, decel, maxSteer, steer;
    // Start is called before the first frame update
    void Start()
    {
        body = gameObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        var drive = transform.forward * 10f;
        drive.y = body.velocity.y;
        body.velocity = drive;
    }
}
