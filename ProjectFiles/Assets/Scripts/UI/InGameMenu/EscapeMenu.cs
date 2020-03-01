using UnityEngine;

namespace UI.InGameMenu {
    public class EscapeMenu : MonoBehaviour {
        // Private Fields
        private GameObject escmenu;

        private void Start() {
            escmenu = Resources.Load<GameObject>("EscapeMenu");
            escmenu.SetActive(false);
        }

        // Update is called once per frame
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape))
                if (Cursor.visible) {
                    escmenu.SetActive(false);
                    Cursor.visible = false;
                } else {
                    Cursor.visible = true;
                    escmenu.SetActive(true);
                }
        }
    }
}