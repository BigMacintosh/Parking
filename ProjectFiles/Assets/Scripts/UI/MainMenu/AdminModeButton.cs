using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MainMenu {
    public class AdminModeButton : MonoBehaviour {
        public void LoadAdminMode(string sceneName) {
            ClientConfig.GameMode = GameMode.AdminMode;
            SceneManager.LoadScene(sceneName);
        }
    }
}