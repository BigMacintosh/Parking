using UnityEngine;
using UnityEngine.UI;

namespace SceneUtilities
{
    public class RenderToMinimap : MonoBehaviour
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color colour;

        private RectTransform mapRect;
        private GameObject indicator;
        
        public void Start()
        {
            // find the mask and add the sprite as a child
            var mask = GameObject.Find("Minimap Mask");
            indicator = new GameObject($"{name} Indicator");
            indicator.AddComponent<Image>();
            var image = indicator.GetComponent<Image>();
            image.sprite = sprite;
            indicator.transform.SetParent(mask.transform);

            mapRect = mask.transform.GetChild(0).GetComponent<RectTransform>();
        }

        public void Update()
        {
        }
    }
}

