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
        body.velocity = new Vector3(0f, body.velocity.y, 10f);
    }
}
