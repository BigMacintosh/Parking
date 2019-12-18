using UnityEngine;

namespace Vehicle
{
    public class DriveController : MonoBehaviour
    {
        private CarFactory carFactory;
        private CarProperties carProperties;
        private float maxMotorTorque = 10000;
        private float maxSteeringAngle = 30;

        private Rigidbody rb;
        private float v;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            v = rb.velocity.magnitude * 3.6f; // km/h
            carFactory = GetComponent<CarFactory>();
            carProperties = carFactory.carProperties;
        }

        private void FixedUpdate()
        {
            var torque = Input.GetAxis("Vertical") * maxMotorTorque;
            var steering = Input.GetAxis("Horizontal") * maxSteeringAngle / TurnMultiplier(v);
            foreach (var axle in carFactory.axles)
            {
                if (axle.Steering)
                {
                    axle.LeftWheel.steerAngle = steering;
                    axle.RightWheel.steerAngle = steering;
                }

                if (axle.Motor)
                {
                    axle.LeftWheel.motorTorque = torque;
                    axle.RightWheel.motorTorque = torque;
                }
            }
        }

        private float TurnMultiplier(float v)
        {
            if (v > 20) return 4;
            if (v > 40) return 5;
            if (v > 80) return 7;
            if (v > 200) return 10;
            if (v > 400) return 20;
            return 1;
        }
    }
}
