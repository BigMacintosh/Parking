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

        public float maxSpeed, accel, maxSteer, steer, drift;

        private float curSpeed;
        
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
            
            if (Input.GetAxis("Vertical") != 0 && grounded)
            {
                body.AddForce(transform.forward * body.mass * accel * Input.GetAxis("Vertical")*4f);
            }


            if (Input.GetAxis("Horizontal") != 0 && grounded)
            {
                
                if (body.angularVelocity.magnitude < 0.8f)
                {
                    body.AddTorque(transform.up * Input.GetAxis("Horizontal") * steer * 400f);
                    body.velocity = getForward() + getSide()*0.7f;

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

        Vector3 getForward()
        {
            return transform.forward * Vector3.Dot(body.velocity, transform.forward);
        }

        Vector3 getSide()
        {
            return transform.right * Vector3.Dot(body.velocity, transform.right);
        }
    }
}
