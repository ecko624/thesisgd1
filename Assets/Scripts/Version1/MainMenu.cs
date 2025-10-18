using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void PlayVersion1()
    {
        SceneManager.LoadSceneAsync("Version1"); 
    }

    public void PlayVersion2()
    {
        SceneManager.LoadSceneAsync("Version2"); 
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
