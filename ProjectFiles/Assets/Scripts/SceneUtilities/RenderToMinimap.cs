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
            // indicators can be added at any stage in the game, so we need to make sure the map exists first
            // TODO: have some sort of global accessible variable (e.g. MapExists) rather than constantly checking
            // if the map exists every frame
            if (!setup)
            {
                var mask = GameObject.Find("Minimap Mask");
                if (mask == null)
                {
                    return;
                }
                // create an image, then add it as a child to the minimap mask
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
            indicatorTransform.localPosition = new Vector2(mapPosition.x - transform.position.x / scroller.MapScale.x, mapPosition.y - transform.position.z / scroller.MapScale.y);
        }
    }
}

