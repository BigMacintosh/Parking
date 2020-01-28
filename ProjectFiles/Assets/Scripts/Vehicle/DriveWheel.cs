using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Vehicle
{
    public class DriveWheel : MonoBehaviour
    {

        private float groundDist;
        private RaycastHit hit;

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            throw new NotImplementedException();
        }

        // Start is called before the first frame update
        void Start()
        {
            groundDist = gameObject.GetComponent<Collider>().bounds.max.y -
                         gameObject.GetComponent<Collider>().bounds.center.y + 0.1f;
        }

        public bool CheckGround()
        {
            Debug.DrawRay(transform.position, -Vector3.up * groundDist, Color.blue, 1f, false);
            return Physics.Raycast(new Ray(transform.position, -Vector3.up), out hit, groundDist);

        }
    }
}
