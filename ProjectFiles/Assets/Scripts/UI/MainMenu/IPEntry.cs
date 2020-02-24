using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;

namespace UI.MainMenu
{
    public class IPEntry : MonoBehaviour
    {
        // It is actually used.
        public void UpdateIP(string ip)
        {
            ClientConfig.PlayerName = ip;
        }
    }
}
