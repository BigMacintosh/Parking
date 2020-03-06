using System;
using UnityEngine;

namespace Game.Core.Driving {
    public class DriveWheel : MonoBehaviour {
        // Serializable Fields
        [SerializeField] private LayerMask mask;

        // Private Fields
        private float      groundDist;
        private RaycastHit hit;

        // Start is called before the first frame update
        private void Start() {
            groundDist = gameObject.GetComponent<Collider>().bounds.max.y -
                         gameObject.GetComponent<Collider>().bounds.center.y + 0.1f;
        }

        public bool CheckGround() {
            Debug.DrawRay(transform.position, -Vector3.up * groundDist, Color.blue, 1f, false);
            return Physics.Raycast(new Ray(transform.position, -Vector3.up), out hit, groundDist, mask);
        }
    }
}