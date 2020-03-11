using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine.UIElements;

namespace Game.Core.Driving {
    public class VehicleDriver : MonoBehaviour {
        // Public Fields
        public List<WheelTransformPair> driveWheels;
        public List<WheelTransformPair> otherWheels;
        public LayerMask                mask;

        public  float maxSteerAngle;
        public  float motorForce;

        // Private Fields
        private bool      acceptinput;
        private Rigidbody body;

        private float horizontalInput;
        private float verticalInput;
        private float drift;
        private bool  jump;


        private float collisionAmplifier = 50f;
        private float collisionCooldown  = 0.5f;
        private float timestamp          = 0f;

        // Start is called before the first frame update
        private void Start() {
            body              = gameObject.GetComponent<Rigidbody>();
            body.centerOfMass = transform.Find("centreOfMass").transform.localPosition;
        }

        private void FixedUpdate() {
            bool isGrounded = CheckGrounded();

            GetInput();
            if (jump && isGrounded) Jump();
            Steer();
            Accelerate();
            UpdateWheels();

            SelfRight();
        }

        public void setAcceptInput(bool option) {
            acceptinput = option;
        }

        bool CheckGrounded() {
            bool isGrounded = false;
            foreach (WheelTransformPair wheel in driveWheels) {
                if (wheel.wheel.isGrounded) {
                    isGrounded = true;
                }
            }

            return isGrounded;
        }

        private void GetInput() {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput   = Input.GetAxis("Vertical");
            drift           = Input.GetAxis("Drift");
            if (Input.GetAxis("Jump") != 0) {
                jump = true;
            }
        }

        private void Jump() {
            body.AddForce(body.mass * motorForce / 3f * transform.up);
            jump = false;
        }

        private void Steer() {
            foreach (WheelTransformPair wheel in driveWheels) {
                wheel.wheel.steerAngle = (maxSteerAngle              * horizontalInput) +
                                         (maxSteerAngle / 2f * drift * (horizontalInput / Mathf.Abs(horizontalInput)));
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

            foreach (WheelTransformPair wheel in otherWheels) {
                UpdateWheel(wheel.wheel, wheel.graphics);
            }
        }

        private void UpdateWheel(WheelCollider wheel, Transform graphics) {
            Vector3    pos = graphics.position;
            Quaternion rot = graphics.rotation;

            wheel.GetWorldPose(out pos, out rot);
            graphics.position = pos;
            graphics.rotation = rot;
        }

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