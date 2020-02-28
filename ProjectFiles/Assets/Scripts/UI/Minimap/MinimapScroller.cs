using UnityEngine;

namespace UI.Minimap {
    public class MinimapScroller : MonoBehaviour {
        // Public Fields
        public Vector2   MapScale       { get; private set; }
        public Transform ObjectToFollow { get; set; }
        public Vector3   MapPosition    => transform.localPosition;

        // Serializable Fields
        [field: Header("World Anchor Points")]
        [field: SerializeField]
        public Vector2 TopLeft { get; } = new Vector2(-250, 250);

        [field: SerializeField] public Vector2 BottomRight { get; } = new Vector2(250, -250);

        // Private Fields
        private Transform parentTransform;

        /// <summary>
        ///     The unit-to-pixel scale between the world (X, Z) and the map (X, Y).
        /// </summary>

        // init in Awake rather than Start because the .localPosition needs to be set up first
        public void Awake() {
            var rect = ((RectTransform) transform).rect;
            MapScale = new Vector2 {
                x = (BottomRight.x - TopLeft.x)     / rect.width,
                y = (TopLeft.y     - BottomRight.y) / rect.height
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