using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadSceneButton : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        //sceneName = "TestScene";
        SceneManager.LoadScene(sceneName);
    }
}
