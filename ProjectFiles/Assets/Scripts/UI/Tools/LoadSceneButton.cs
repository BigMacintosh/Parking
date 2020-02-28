using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Tools {
    public class LoadSceneButton : MonoBehaviour {
        public void LoadScene(string sceneName) {
            SceneManager.LoadScene(sceneName);
        }
    }
}