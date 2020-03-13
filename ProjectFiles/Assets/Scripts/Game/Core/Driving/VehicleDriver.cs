﻿using UnityEngine;

namespace Game.Core.Driving {
    public class VehicleDriver : MonoBehaviour {
        // Public Fields
        public float                                       maxSpeed;
        public float                                       accel;
        public float                                       maxSteer;
        public float                                       driftFactor;
        public System.Collections.Generic.List<DriveWheel> driveWheels;
        public LayerMask                                   mask;

        // Private Fields
        private bool      acceptinput;
        private Rigidbody body;
        private float     turn;
        private float     maxTurn;
        private float     collisionAmplifier = 50f;
        private float     collisionCooldown  = 0.5f;
        private float     timestamp          = 0f;

        // Start is called before the first frame update
        private void Start() {
            body              = gameObject.GetComponent<Rigidbody>();
            body.centerOfMass = transform.Find("centreOfMass").transform.localPosition;
        }

        private void FixedUpdate() {
            var grounded = false;
            turn = maxSteer * Input.GetAxis("Horizontal");

            foreach (var wheel in driveWheels) {
                grounded                                 = wheel.CheckGround() ? true : grounded;
                wheel.gameObject.transform.localRotation = Quaternion.Euler(0, turn, 0);
            }

            //Driving Forces
            if (acceptinput && Input.GetAxis("Vertical") != 0 && grounded && Mathf.Abs(GetForward()) < maxSpeed) {
                body.AddForce(body.mass * accel * Input.GetAxis("Vertical") * transform.forward);
            }

            //Turning Forces
            if (Input.GetAxis("Horizontal") != 0 && grounded && body.angularVelocity.magnitude < maxSteer / 30f &&
                Mathf.Abs(GetForward())     > 0.5f) {
                body.AddTorque(turn * body.mass * 2f * (GetForward() / Mathf.Abs(GetForward())) * transform.up);
            }

            if (acceptinput && Input.GetAxis("Jump") != 0 && grounded) {
                body.AddForce(body.mass * accel * Input.GetAxis("Jump") * transform.up * 10f);
            }

            //Drifting
            if (grounded) {
                if (Input.GetAxis("Drift") != 0) {
                    maxTurn = maxSteer * driftFactor / 30f;
                } else {
                    maxTurn       =  maxSteer          / 30f;
                    body.velocity -= GetSide() * 0.25f * transform.right;
                }
            }

            //Self-Righting
            if (Mathf.Abs(body.rotation.eulerAngles.z) > 45 && Mathf.Abs(body.rotation.eulerAngles.z) < 315 ||
                Mathf.Abs(body.rotation.eulerAngles.x) > 60 && Mathf.Abs(body.rotation.eulerAngles.x) < 300) {
                var targetRotation = Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(body.transform.forward,
                                           Vector3.up));

                body.MoveRotation(Quaternion.RotateTowards(
                                      body.rotation, targetRotation, Time.fixedDeltaTime * 90f));
            }
        }

        private float GetForward() {
            return Vector3.Dot(body.velocity, transform.forward);
        }

        private float GetSide() {
            return Vector3.Dot(body.velocity, transform.right);
        }

        public void setAcceptInput(bool option) {
            acceptinput = option;
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