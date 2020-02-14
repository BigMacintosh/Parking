using UnityEngine;

namespace SceneUtilities
{
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] private Vector3 offset;

        public Transform ObjectToFollow { get; set; }

        public void Start()
        {
            ObjectToFollow = FindObjectOfType<Game.Game>().transform;
        }

        public void FixedUpdate()
        {
            transform.position = ObjectToFollow.position + offset;
            transform.rotation = Quaternion.Euler(90, ObjectToFollow.eulerAngles.y, 0);
        }
    }
}