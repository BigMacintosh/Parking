using UnityEngine;

namespace Car
{
    public class AntiRoll : MonoBehaviour
    {
        [SerializeField] private WheelCollider leftWheel;
        [SerializeField] private WheelCollider rightWheel;
        [SerializeField] private GameObject car;
        [SerializeField] private float antiSwayMultiplier = 5000.0f;
        private Rigidbody rb;
        
        private void Start()
        {
            rb = car.GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            var travelL = 1.0f;
            var travelR = 1.0f;

            WheelHit hit;
            var groundedL = leftWheel.GetGroundHit(out hit);
            if (groundedL)
            {
                travelL = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius) 
                          / leftWheel.suspensionDistance;
            }

            var groundedR = rightWheel.GetGroundHit(out hit);
            if (groundedR)
            {
                travelR = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius) 
                          / rightWheel.suspensionDistance;
            }

            var antiRollForce = (travelL - travelR) * antiSwayMultiplier;
            if (groundedL)
            {
                rb.AddForceAtPosition(leftWheel.transform.up * -antiRollForce, leftWheel.transform.position);
            }

            if (groundedR)
            {
                rb.AddForceAtPosition(rightWheel.transform.up * antiRollForce, rightWheel.transform.position);
            }
        }
    }
}

