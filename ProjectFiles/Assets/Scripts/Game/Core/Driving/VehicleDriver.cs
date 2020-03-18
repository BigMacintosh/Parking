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

        public float maxSteerAngle;
        public float motorForce;
        public float roadMult;

        // Private Fields
        private bool      acceptinput;
        private Rigidbody body;

        private float horizontalInput;
        private float verticalInput;
        private bool  drift;
        private bool  jump;
        private bool  reset;

        private bool isGrounded;
        private bool roadBelow;

        private bool  canReset      = true;
        private float resetTime     = 0f;
        private float resetCooldown = 5f;


        private float collisionAmplifier = 50f;
        private float collisionCooldown  = 0.5f;
        private float collisionTime      = 0f;

        private WheelFrictionCurve driftCurve = new WheelFrictionCurve();
        private WheelFrictionCurve stdCurve;

        // Start is called before the first frame update
        private void Start() {
            resetTime = Time.time + resetCooldown;
            body              = gameObject.GetComponent<Rigidbody>();
            body.centerOfMass = transform.Find("centreOfMass").transform.localPosition;

            driftCurve.extremumSlip   = 0.15f;
            driftCurve.extremumValue  = 0.2f;
            driftCurve.asymptoteSlip  = 1f;
            driftCurve.asymptoteValue = 0.2f;
            driftCurve.stiffness      = 1f;

            stdCurve = driveWheels[0].wheel.sidewaysFriction;
        }

        private void FixedUpdate() {
            CheckGrounded();
            CheckReset();
            GetInput();
            if (jump && isGrounded) Jump();
            else {
                jump = false;
            }
            if (reset && canReset) ResetPos();
            else {
                reset = false;
            }
            Steer();
            Accelerate();
            UpdateWheels();

            SelfRight();
        }

        public void setAcceptInput(bool option) {
            acceptinput = option;
        }

        private void CheckGrounded() {
            isGrounded = false;
            roadBelow  = false;
            foreach (WheelTransformPair wheel in driveWheels) {
                if (wheel.wheel.isGrounded) {
                    isGrounded = true;
                }

                RaycastHit hit;
                if (Physics.Raycast(wheel.graphics.position, Vector3.down, out hit)) {
                    roadBelow = hit.collider.gameObject.tag == "Road";
                }
            }
        }

        private void CheckReset() {
            
        }

        private void GetInput() {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput   = Input.GetAxis("Vertical");
            if (Input.GetAxis("Jump") != 0) {
                jump = true;
            }

            if (Input.GetAxis("Drift") != 0) {
                drift = true;
            } else {
                drift = false;
            }

            if (Input.GetAxis("Reset") != 0) {
                reset = true;
            }
        }

        private void Jump() {
            body.AddForce(body.mass * motorForce / 5f * transform.up);
            jump = false;
        }

        private void ResetPos() {
            
            Debug.Log("RESET");
            canReset  = false;
            resetTime = Time.time + resetCooldown;
            reset = false;
        }

        private void Steer() {
            foreach (WheelTransformPair wheel in driveWheels) {
                wheel.wheel.steerAngle = (maxSteerAngle * horizontalInput);
                if (drift) {
                    wheel.wheel.sidewaysFriction = driftCurve;
                } else {
                    wheel.wheel.sidewaysFriction = stdCurve;
                }
            }

            foreach (WheelTransformPair wheel in otherWheels) {
                if (drift) {
                    wheel.wheel.sidewaysFriction = driftCurve;
                } else {
                    wheel.wheel.sidewaysFriction = stdCurve;
                }
            }
        }

        private void Accelerate() {
            foreach (WheelTransformPair wheel in driveWheels) {
                wheel.wheel.motorTorque =
                    roadBelow ? motorForce * verticalInput * roadMult : motorForce * verticalInput;
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
            if ((mask.value & 1 << other.gameObject.layer) != 0 && collisionTime < Time.time) {
                Rigidbody otherBody = other.gameObject.GetComponent<Rigidbody>();
                Vector3   colDir    = Vector3.Normalize(transform.position - other.transform.position);

                float myMomentum    = Mathf.Abs(Vector3.Dot(body.velocity,      colDir)) * body.mass;
                float otherMomentum = Mathf.Abs(Vector3.Dot(otherBody.velocity, colDir)) * otherBody.mass;

                if (myMomentum < otherMomentum) {
                    body.AddForce(colDir * otherMomentum * collisionAmplifier);
                }

                collisionTime = Time.time + collisionCooldown;
            }
        }
    }
}