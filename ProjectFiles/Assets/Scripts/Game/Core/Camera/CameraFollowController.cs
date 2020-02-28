using UnityEngine;

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

        private void LookAtTarget() {
            var _lookDirection = ObjectToFollow.position - transform.position;
            var _rot           = Quaternion.LookRotation(_lookDirection, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);
        }

        private void MoveToTarget() {
            var _targetPos = ObjectToFollow.position           +
                             ObjectToFollow.forward * offset.z +
                             ObjectToFollow.right   * offset.x +
                             ObjectToFollow.up      * offset.y;
            transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);
        }

        private void FixedUpdate() {
            LookAtTarget();
            MoveToTarget();
        }
    }
}