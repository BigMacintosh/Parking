using UnityEngine;

namespace SceneUtilities
{
    public class MinimapScroller : MonoBehaviour
    {
        [field: Header("World Anchor Points")]
        [field: SerializeField] public Vector2 TopLeft { get; private set; } = new Vector2(-250, 250);
        [field: SerializeField] public Vector2 BottomRight { get; private set; } = new Vector2(250, -250);

        public Vector2 AnchorDifference { get; private set; }
        public Transform ObjectToFollow { get; set; }

        private RectTransform rectTransform;

        public void Awake()
        {
            AnchorDifference = TopLeft - BottomRight;
            ObjectToFollow = FindObjectOfType<Game.Game>().transform;
            rectTransform = ((RectTransform) transform);
        }

        public void Update()
        {
            // calculate the proportion of the map that you've travelled across from the top left
            // this probs could be simplified but DON'T TOUCH
            // NOTE: 2D Y coord = 3D Z coord
            var xRatio = 1 - (TopLeft.x - ObjectToFollow.position.x) / AnchorDifference.x;
            var yRatio = 1 - (TopLeft.y - ObjectToFollow.position.z) / AnchorDifference.y;
            var rect = rectTransform.rect;
            transform.localPosition = new Vector2(rect.width / 2 - xRatio * rect.width, -rect.height / 2 + yRatio * rect.height);
        }
    }
}

