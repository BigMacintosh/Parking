﻿using UnityEngine;
using UnityEngine.UI;

namespace SceneUtilities
{
    public class RenderToMinimap : MonoBehaviour
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color colour;
        [SerializeField] private Vector2 size = new Vector2(16, 16);

        private MinimapScroller scroller;
        private GameObject indicator;
        private RectTransform indicatorTransform;
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
                indicatorTransform = (RectTransform) indicator.transform;
                indicatorTransform.sizeDelta = size;
                scroller = mask.transform.GetChild(0).GetComponent<MinimapScroller>();
                setup = true;
            }
            var mapPosition = scroller.MapPosition;
            var trans = transform;
            var position = trans.position;
            indicatorTransform.localPosition = new Vector2(mapPosition.x - position.x / scroller.MapScale.x, mapPosition.y - position.z / scroller.MapScale.y);
            indicatorTransform.localRotation = Quaternion.Euler(0, 0, -trans.rotation.eulerAngles.y);
        }
    }
}
