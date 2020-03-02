using UnityEngine;

namespace UI.Minimap {
    public class MinimapScroller : MonoBehaviour {
        public Vector2   MapScale       { get; private set; }
        public Transform ObjectToFollow { get; set; }
        /// <summary>
        /// The unit-to-pixel scale between the world (X, Z) and the map (X, Y).
        /// </summary>
        public Vector3 MapPosition => transform.localPosition;
        
        [Header("World Anchor Points")]
        [SerializeField] private Vector2 topLeft;
        [SerializeField] private Vector2 bottomRight;
        private Transform parentTransform;

        // init in Awake rather than Start because the .localPosition needs to be set up first
        public void Awake() {
            var rect = ((RectTransform) transform).rect;
            MapScale = new Vector2 {
                x = (bottomRight.x - topLeft.x)     / rect.width,
                y = (topLeft.y     - bottomRight.y) / rect.height
            };
            ObjectToFollow  = FindObjectOfType<Game.Main.Game>().transform;
            parentTransform = transform.parent;
        }

        public void Update() {
            var position = ObjectToFollow.position;
            transform.localPosition  = new Vector2(position.x / MapScale.x, position.z / MapScale.y);
            parentTransform.rotation = Quaternion.Euler(0, 0, ObjectToFollow.rotation.eulerAngles.y);
        }
    }
}