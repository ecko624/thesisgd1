using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadtimer : MonoBehaviour
{
    // Set your delay time (in seconds)
    public float delayTime = 5f;

    // The name of the scene to load
    public string nextSceneName = "NextScene";

    void Start()
    {
        // Start the timer
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        // Wait for the delay time
        yield return new WaitForSeconds(delayTime);

        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }
}
