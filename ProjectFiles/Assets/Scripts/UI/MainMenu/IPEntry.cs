using Network;
using UnityEngine;

namespace UI.MainMenu {
    public class IPEntry : MonoBehaviour {
        public void UpdateIP(string ip) {
            ClientConfig.PlayerName = ip;
        }
    }
}