using UnityEngine;

namespace UI.Minimap {
    public class MinimapController : MonoBehaviour {
        // Public Fields
        public Transform ObjectToFollow { get; set; }
        
        // Serializable Fields
        [SerializeField] private Vector3 offset;

        

        public void Start() {
            ObjectToFollow = FindObjectOfType<Game.Main.Game>().transform;
        }

        public void FixedUpdate() {
            // follow the object at a fixed offset, and lock the X/Z rotation
            transform.position = ObjectToFollow.position + offset;
            transform.rotation = Quaternion.Euler(90, ObjectToFollow.eulerAngles.y, 0);
        }
    }
}