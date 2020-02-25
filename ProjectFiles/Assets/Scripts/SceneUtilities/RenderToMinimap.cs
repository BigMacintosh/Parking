using UnityEngine;
using UnityEngine.UI;

namespace SceneUtilities
{
    public class RenderToMinimap : MonoBehaviour
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color colour;

        private MinimapScroller scroller;
        private GameObject indicator;
        private Transform indicatorTransform;
        private bool setup;

        public void Update()
        {
            if (!setup)
            {
                // find the mask and add the sprite as a child
                var mask = GameObject.Find("Minimap Mask");
                if (mask == null)
                {
                    return;
                }
                indicator = new GameObject($"{name} Indicator");
                indicator.AddComponent<Image>();
                var image = indicator.GetComponent<Image>();
                image.sprite = sprite;
                indicator.transform.SetParent(mask.transform);
                indicatorTransform = indicator.transform;
                scroller = mask.transform.GetChild(0).GetComponent<MinimapScroller>();
                setup = true;
            }
            var mapPosition = scroller.MapPosition;
            indicatorTransform.localPosition = new Vector2(mapPosition.x - transform.position.x / scroller.XScale, mapPosition.y - transform.position.z / scroller.YScale);
        }
    }
}

