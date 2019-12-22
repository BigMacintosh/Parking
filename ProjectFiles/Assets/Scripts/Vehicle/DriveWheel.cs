using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle
{
    public class DriveWheel : MonoBehaviour
    {

        private float groundDist;
        // Start is called before the first frame update
        void Start()
        {
            groundDist = gameObject.GetComponent<Collider>().bounds.max.y - gameObject.GetComponent<Collider>().bounds.center.y + 0.1f;
        }

        public bool CheckGround()
        {
            Debug.DrawRay(transform.position, -Vector3.up * groundDist, Color.blue, 1f, false);
            return Physics.Raycast(transform.position, -Vector3.up, groundDist);

        }
    }
}
