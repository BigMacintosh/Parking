using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle
{
    public class VehicleDriver : MonoBehaviour
    {
        private Rigidbody body;

        //Car Properties

        public float maxSpeed, accel, maxSteer, steer;

        private float curSpeed;

    // Start is called before the first frame update
        void Start()
        {
            body = gameObject.GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            Debug.Log("Assigning Speed: " + curSpeed);
            if (Input.GetAxis("Vertical") != 0)
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
        }
    }
}
