using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    public static bool IsGamePaused { get; private set; } = false;

    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;
    }
    
    public void GoToMainMenu()
    {
        PauseController.SetPause(false); // Ensure game is unpaused before loading
        SceneManager.LoadScene("MainMenu"); // Replace with your main menu scene name
    }
}
