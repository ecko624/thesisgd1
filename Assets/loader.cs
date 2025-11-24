using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loader : MonoBehaviour
{
    public string nexr;
    
    public void Start2()
    {
        Debug.Log($"Attempting to load scene: '{nexr}'");
        Debug.Log($"Total scenes in build: {SceneManager.sceneCountInBuildSettings}");
        
        try
        {
            SceneManager.LoadScene(nexr);
            Debug.Log($"Successfully loaded scene: {nexr}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene '{nexr}': {e.Message}");
        }
    }

    void Update()
    {
        
    }
}
