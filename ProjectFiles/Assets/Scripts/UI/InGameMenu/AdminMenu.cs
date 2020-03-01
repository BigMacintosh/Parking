using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI.InGameMenu {
    public class AdminMenu : MonoBehaviour {
        // Delegates
        public event TriggerGameStartDelegate TriggerGameStartEvent;

        // Serializable Fields
        [SerializeField] private GameObject adminMenu;
        [SerializeField] private Button     startGameButton;

        // Private Fields
        private bool isServerMode;


        private void Start() {
            adminMenu.SetActive(ClientConfig.GameMode == GameMode.AdminMode || isServerMode);
        }

        public void OnGameStart() {
            startGameButton.interactable = false;
        }

        public void SetServerMode(bool isServerMode) {
            this.isServerMode = isServerMode;
            if (!isServerMode) return;
            if (!(adminMenu is null)) adminMenu.SetActive(true);
        }

        public void StartGame() {
            Debug.Log("Admin Client: StartGame button pressed");
            TriggerGameStartEvent?.Invoke();
        }
    }
}