using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;


namespace UI.MainMenu
{
    public class NameEntry : MonoBehaviour
    {
        // It is actually used.
        public void UpdateName(string playerName)
        {
            ClientConfig.PlayerName = playerName;
        }
    }
}
