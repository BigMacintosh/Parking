using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfRighting : MonoBehaviour
{
    public GameObject car;
    private Rigidbody rb;
    private Collider carCollider;
    public float distToGround;

    // Start is called before the first frame update
    void Start()
    {
        rb = car.GetComponent<Rigidbody>();
        carCollider = car.GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(IsGrounded())
        {

        }
    }
    public bool IsGrounded()
    {
        return Physics.CheckCapsule(carCollider.bounds.center, new Vector3(carCollider.bounds.center.x, carCollider.bounds.min.y - 0.1f, carCollider.bounds.center.z), 0.18f);
    }
}

