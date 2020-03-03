using UnityEngine;

namespace UI.Minimap {
    public class MinimapScroller : MonoBehaviour {
        /// <summary>
        /// The unit-to-pixel scale between the world (X, Z) and the map (X, Y).
        /// e.g. World * UnitPixelScale = Map
        /// </summary>
        public Vector2 UnitPixelScale { get; private set; }

        public Transform ObjectToFollow { get; set; }
        public Vector3   MapPosition    => transform.localPosition;

        [SerializeField] private Vector2   worldSize;
        private                  Transform parentTransform;

        // init in Awake rather than Start because the .localPosition needs to be set up first
        public void Awake() {
            if (worldSize.x < Vector2.kEpsilon || worldSize.y < Vector2.kEpsilon) {
                worldSize = new Vector2(500, 500);
                Debug.LogError($"Invalid world size, defaulting to {worldSize}. "
                             + $"Ensure that the world size > {Vector2.kEpsilon}.");
            }

            var rect = ((RectTransform) transform).rect;
            // since we're translating the map, player moves right => map moves left and vice versa... so we need 
            // the NEGATIVE scale
            UnitPixelScale  = -rect.size / worldSize;
            ObjectToFollow  = FindObjectOfType<Game.Main.Game>().transform;
            parentTransform = transform.parent;
        }

        public void Update() {
            var position = ObjectToFollow.position;
            transform.localPosition  = new Vector2(position.x * UnitPixelScale.x, position.z * UnitPixelScale.y);
            parentTransform.rotation = Quaternion.Euler(0, 0, ObjectToFollow.rotation.eulerAngles.y);
        }
    }
}