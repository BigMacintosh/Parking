using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRoll : MonoBehaviour
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public GameObject car;
    private Rigidbody rb;

    [SerializeField] public float AntiSwayMultiplier = 5000.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = car.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        WheelHit hit = new WheelHit();
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = leftWheel.GetGroundHit(out hit);

        if (groundedL)
        {
            travelL = (-leftWheel.transform.InverseTransformPoint(hit.point).y
                    - leftWheel.radius) / leftWheel.suspensionDistance;
        }

        bool groundedR = rightWheel.GetGroundHit(out hit);

        if (groundedR)
        {
            travelR = (-rightWheel.transform.InverseTransformPoint(hit.point).y
                    - rightWheel.radius) / rightWheel.suspensionDistance;
        }

        var antiRollForce = (travelL - travelR) * AntiSwayMultiplier;

        if (groundedL)
            rb.AddForceAtPosition(leftWheel.transform.up * -antiRollForce,
                leftWheel.transform.position);
        if (groundedR)
            rb.AddForceAtPosition(rightWheel.transform.up * antiRollForce,
                rightWheel.transform.position);
    }
}

