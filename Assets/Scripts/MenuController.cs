using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    // Start is called before the first frame update
    void Start()
    {
        menuCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!menuCanvas.activeSelf && PauseController.IsGamePaused)
            {
                return; // Prevent opening menu if game is paused
            }
            menuCanvas.SetActive(!menuCanvas.activeSelf);
            PauseController.SetPause(menuCanvas.activeSelf);
        }
    }

        public void GoToMainMenu()
    {
        PauseController.SetPause(false); // Ensure game is unpaused before loading
        SceneManager.LoadScene("SampleScene"); // Replace with your main menu scene name
    }
    
}


