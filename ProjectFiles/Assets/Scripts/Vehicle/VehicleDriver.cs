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

        public float maxSpeed, accel, maxSteer, steer;

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
                curSpeed = Input.GetAxis("Vertical") > 0 ? Mathf.Min(curSpeed + Input.GetAxis("Vertical") * accel * Time.deltaTime, maxSpeed) : Mathf.Max(curSpeed + Input.GetAxis("Vertical") * accel * Time.deltaTime, -maxSpeed);

                Vector3 vel = new Vector3(0f, body.velocity.y, 0f) + transform.forward * curSpeed;

                body.velocity = vel;
            }
            else
            {
                
                curSpeed += curSpeed > 0 ? - accel * Time.deltaTime : accel * Time.deltaTime;
                

                Vector3 vel = new Vector3(0f, body.velocity.y, 0f) + transform.forward * curSpeed;

                body.velocity = vel;
            }

            if (Mathf.Abs(body.rotation.eulerAngles.z) > 45)
            {
                var targetRotation = Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(body.transform.forward,
                        Vector3.up));

                body.MoveRotation(Quaternion.RotateTowards(
                    body.rotation, targetRotation, Time.fixedDeltaTime * 90f));
            }
        }
    }
}
