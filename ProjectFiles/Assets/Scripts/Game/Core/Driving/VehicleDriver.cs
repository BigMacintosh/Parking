using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Driving {
    public class VehicleDriver : MonoBehaviour {
        // Public Fields
        public List<WheelCollider> frontWheels;
        public List<WheelCollider> rearWheesl;
        public List<Transform>     frontWheelGraphics;
        public List<Transform>     rearWheelGraphics;
        public LayerMask                                      mask;
        
        public float                                          maxSteerAngle;
        public float                                          motorForce;

        // Private Fields
        private bool      acceptinput;
        private Rigidbody body;
        
        private float     horizontalInput;
        private float     verticalInput;
        
        private float     collisionAmplifier = 50f;
        private float     collisionCooldown  = 0.5f;
        private float     timestamp          = 0f;

        // Start is called before the first frame update
        private void Start() {
            body              = gameObject.GetComponent<Rigidbody>();
            body.centerOfMass = transform.Find("centreOfMass").transform.localPosition;
        }

        private void FixedUpdate() {
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