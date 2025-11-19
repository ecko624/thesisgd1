using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void PlayVersion1()
    {
        SceneManager.LoadSceneAsync("IntroCutscene"); 
    }

    public void PlayVersion2()
    {
        SceneManager.LoadSceneAsync("IntroCutscene1"); 
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
