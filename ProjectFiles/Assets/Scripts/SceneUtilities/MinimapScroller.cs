using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneUtilities
{
    public class MinimapScroller : MonoBehaviour
    {
        [field: Header("World Anchor Points")] 
        [field: SerializeField] public Vector2 topLeft { get; private set; } = new Vector2(250, -250);
        [field: SerializeField] public Vector2 bottomRight { get; private set;  } = new Vector2(-250, 250);

        public Vector2 anchorDifference { get; private set; }
        
        public void Start()
        {
            anchorDifference = topLeft - bottomRight;
            // top left of image is (width / 2), (-height / 2)
            // bot right of image is (-width / 2), (height / 2)
        }

        public void Update()
        {
            
        }
    }

}

