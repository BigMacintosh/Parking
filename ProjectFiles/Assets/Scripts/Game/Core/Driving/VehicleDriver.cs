using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Game.Core.Driving {
    public class VehicleDriver : MonoBehaviour {
        // Public Fields
        public List<WheelTransformPair> driveWheels;
        public List<WheelTransformPair> otherWheels;
        public LayerMask                                    mask;

        public float maxSteerAngle;
        public float motorForce;

        // Private Fields
        private bool      acceptinput;
        private Rigidbody body;

        private float horizontalInput;
        private float verticalInput;
        private float steeringAngle;

        private float collisionAmplifier = 50f;
        private float collisionCooldown  = 0.5f;
        private float timestamp          = 0f;

        // Start is called before the first frame update
        private void Start() {
            body              = gameObject.GetComponent<Rigidbody>();
            body.centerOfMass = transform.Find("centreOfMass").transform.localPosition;
        }

        private void FixedUpdate() {
            GetInput();
            Steer();
            Accelerate();
            UpdateWheels();

            SelfRight();
        }

        public void setAcceptInput(bool option) {
            acceptinput = option;
        }

        private void GetInput() {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput   = Input.GetAxis("Vertical");
        }

        private void Steer() {
            foreach (WheelTransformPair wheel in driveWheels) {
                wheel.wheel.steerAngle = maxSteerAngle * horizontalInput;
            }
        }

        private void Accelerate() {
            foreach (WheelTransformPair wheel in driveWheels) {
                wheel.wheel.motorTorque = motorForce * verticalInput;
            }
        }

        private void UpdateWheels() {
            foreach (WheelTransformPair wheel in driveWheels) {
                UpdateWheel(wheel.wheel, wheel.graphics);
            }
        }

        private void UpdateWheel(WheelCollider wheel, Transform graphics) { }

        private void SelfRight() {
            if (Mathf.Abs(body.rotation.eulerAngles.z) > 45 && Mathf.Abs(body.rotation.eulerAngles.z) < 315 ||
                Mathf.Abs(body.rotation.eulerAngles.x) > 60 && Mathf.Abs(body.rotation.eulerAngles.x) < 300) {
                var targetRotation = Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(body.transform.forward,
                                           Vector3.up));

                body.MoveRotation(Quaternion.RotateTowards(
                                      body.rotation, targetRotation, Time.fixedDeltaTime * 90f));
            }
        }

        private void OnCollisionEnter(Collision other) {
            if ((mask.value & 1 << other.gameObject.layer) != 0 && timestamp < Time.time) {
                Rigidbody otherBody = other.gameObject.GetComponent<Rigidbody>();
                Vector3   colDir    = Vector3.Normalize(transform.position - other.transform.position);

                float myMomentum    = Mathf.Abs(Vector3.Dot(body.velocity,      colDir)) * body.mass;
                float otherMomentum = Mathf.Abs(Vector3.Dot(otherBody.velocity, colDir)) * otherBody.mass;

                if (myMomentum < otherMomentum) {
                    body.AddForce(colDir * otherMomentum * collisionAmplifier);
                }

                timestamp = Time.time + collisionCooldown;
            }
        }
    }
}