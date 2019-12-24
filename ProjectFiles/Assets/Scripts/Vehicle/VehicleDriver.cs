using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

namespace Vehicle
{
    public class VehicleDriver : MonoBehaviour
    {
        private Rigidbody body;

        //Car Properties

        public float maxSpeed, accel, maxSteer, steer, driftFactor;

        public List<DriveWheel> driveWheels;

    // Start is called before the first frame update
        void Start()
        {
            body = gameObject.GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            bool grounded = false;
            
            foreach (DriveWheel wheel in driveWheels)
            {
                grounded = wheel.CheckGround() ? true : grounded;
            }
            
            //Driving Forces
            if (Input.GetAxis("Vertical") != 0 && grounded && Mathf.Abs(GetForward()) < maxSpeed)
            {
                body.AddForce(body.mass * accel * Input.GetAxis("Vertical") * transform.forward);
            }

            //Turning Forces
            if (Input.GetAxis("Horizontal") != 0 && grounded && body.angularVelocity.magnitude < 1f)
            {
                body.AddTorque(Input.GetAxis("Horizontal") * steer * (GetForward()/Mathf.Abs(GetForward())) * transform.up);
            }
            
            //Drifting
            if (grounded)
            {
                if (Mathf.Abs(GetForward()) < maxSpeed * driftFactor)
                {
                    body.velocity -= GetSide() * 0.25f * transform.right;
                }
                else
                {
                    Debug.Log("DRIFITING");
                    
                }
            }

            if (Mathf.Abs(body.rotation.eulerAngles.z) > 45 && Mathf.Abs(body.rotation.eulerAngles.z) < 315)
            {
                var targetRotation = Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(body.transform.forward,
                        Vector3.up));

                body.MoveRotation(Quaternion.RotateTowards(
                    body.rotation, targetRotation, Time.fixedDeltaTime * 90f));
            }
        }

        float GetForward()
        {
            return Vector3.Dot(body.velocity, transform.forward);
        }

        float GetSide()
        {
            return Vector3.Dot(body.velocity, transform.right);
        }
    }
}
