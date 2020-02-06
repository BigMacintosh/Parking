using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class EscapeMenu : MonoBehaviour
    {
        // Start is called before the first frame update
        private GameObject escmenu;

        void Start()
        {
            escmenu = Resources.Load<GameObject>("EscapeMenu");
            escmenu.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
                if (Cursor.visible)
                {
                    escmenu.SetActive(false);
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.visible = true;
                    escmenu.SetActive(true);
                }
        }
    }
}