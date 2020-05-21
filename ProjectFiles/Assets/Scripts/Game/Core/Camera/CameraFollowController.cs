using UnityEngine;
using System;

namespace Game.Core.Camera {
    public class CameraFollowController : MonoBehaviour {
        // Public Fields
        public Transform ObjectToFollow { private get; set; }

        // Serializable Fields
        [SerializeField] private float   followSpeed = 10;
        [SerializeField] private float   lookSpeed   = 10;
        [SerializeField] private Vector3 offset;

        public void Start() {
            ObjectToFollow = FindObjectOfType<Main.Game>().transform;
        }

        private Vector3 getDirection() {
            var _lookDirectionx = (ObjectToFollow.position.x - transform.position.x);
            var _lookDirectionz = (ObjectToFollow.position.z - transform.position.z);
            var _lookDirection = new Vector3(_lookDirectionx, 0f, _lookDirectionz).normalized; 
            return _lookDirection;
        }
        private void LookAtTarget() {

            var _rot           = Quaternion.LookRotation(getDirection(), Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);
        }

        private void MoveToTarget() {
            var _targetPos = ObjectToFollow.position           +
                             Vector3.ProjectOnPlane(ObjectToFollow.forward, Vector3.up) * offset.z +
                             Vector3.right   * offset.x +
                             Vector3.up      * offset.y;

            transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);
        }

        private void FixedUpdate() {
            MoveToTarget();
            LookAtTarget();
        }
    }
}