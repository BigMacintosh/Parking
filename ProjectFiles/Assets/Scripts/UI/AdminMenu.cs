using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AdminMenu : MonoBehaviour
    {
        public event TriggerGameStartDelegate TriggerGameStartEvent;
        [SerializeField] private Button startGameButton;
        [SerializeField] private GameObject adminMenu;
        private bool isServerMode;

        void Start()
        {
            adminMenu.SetActive(ClientConfig.GameMode == GameMode.AdminMode || isServerMode);
        }

        public void OnGameStart()
        {
            startGameButton.interactable = false;
        }

        public void SetServerMode(bool isServerMode)
        {
            this.isServerMode = isServerMode;
            if (!isServerMode) return;
            if (!(adminMenu is null)) adminMenu.SetActive(true);
        }

        public void StartGame()
        {
            Debug.Log("Admin Client: StartGame button pressed");
            TriggerGameStartEvent?.Invoke();
        }

    }
}