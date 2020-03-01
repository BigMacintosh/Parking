using Network;
using UnityEngine;

namespace UI.MainMenu {
    public class NameEntry : MonoBehaviour {
        public void UpdateName(string playerName) {
            ClientConfig.PlayerName = playerName;
        }
    }
}