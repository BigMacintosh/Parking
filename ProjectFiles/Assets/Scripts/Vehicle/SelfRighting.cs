using UnityEngine;

namespace Car
{
    public class SelfRighting : MonoBehaviour
    {
        [SerializeField] private GameObject car;
        [SerializeField] private float distToGround;
    
        private Rigidbody rb;
        private Collider carCollider;

        private void Start()
        {
            rb = car.GetComponent<Rigidbody>();
            carCollider = car.GetComponent<Collider>();
        }
    
        private void Update()
        {
            if(IsGrounded())
            {
                // ??
            }
        }
        public bool IsGrounded()
        {
            return Physics.CheckCapsule(carCollider.bounds.center, new Vector3(carCollider.bounds.center.x, carCollider.bounds.min.y - 0.1f, carCollider.bounds.center.z), 0.18f);
        }
    }
}

