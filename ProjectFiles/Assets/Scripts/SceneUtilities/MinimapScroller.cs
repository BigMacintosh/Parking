using UnityEngine;

namespace SceneUtilities
{
    public class MinimapScroller : MonoBehaviour
    {
        [field: Header("World Anchor Points")]
        [field: SerializeField] public Vector2 TopLeft { get; private set; } = new Vector2(-250, 250);
        [field: SerializeField] public Vector2 BottomRight { get; private set; } = new Vector2(250, -250);

        public float XScale { get; private set; }
        public float YScale { get; private set; }
        public Transform ObjectToFollow { get; set; }
        public Vector3 MapPosition => transform.localPosition;

        // private RectTransform rectTransform;

        public void Awake()
        {
            var rect = ((RectTransform) transform).rect;
            XScale = (BottomRight.x - TopLeft.x) / rect.width;
            YScale = (TopLeft.y - BottomRight.y) / rect.height;
            ObjectToFollow = FindObjectOfType<Game.Game>().transform;
        }

        public void Update()
        {
            transform.localPosition = new Vector2(ObjectToFollow.position.x / XScale, ObjectToFollow.position.z / YScale);
        }
    }
}

