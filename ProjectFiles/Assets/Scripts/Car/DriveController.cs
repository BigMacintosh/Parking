using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class DriveController : MonoBehaviour
{
    private Rigidbody rigid;
    [SerializeField] private Transform steering;
    [SerializeField] private float accel; //acceleration value for the car
    [SerializeField] private float maxSpeed; //top speed

    [SerializeField] private float turnFactor;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    // FixedUpdate is 50Hz
    void FixedUpdate()
    {
        if (Input.GetButton("Vertical"))
        {
            rigid.AddForce(steering.forward * accel * Input.GetAxis("Vertical"));
        }

        if (Input.GetButton("Horizontal"))
        {
            steering.Rotate(0f, turnFactor * Input.GetAxis("Horizontal") * Time.deltaTime, 0f);
        }
    }
}
