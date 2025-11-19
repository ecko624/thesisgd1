using UnityEngine;
using UnityEngine.SceneManagement;

public class dont : MonoBehaviour
{
    public AudioSource audioSource; // assign your AudioSource here
    private static dont instance = null; // Static reference for Singleton pattern

    void Awake()
    {
        // 1. Singleton Check: Ensure only one instance exists across scenes
        if (instance == null)
        {
            instance = this;
            // Mark the object to persist across scene changes
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            // If an instance already exists, destroy this duplicate
            Destroy(gameObject);
            return;
        }

        // 2. Initial Scene Check (In case the game starts on a forbidden scene)
        CheckCurrentSceneForDestruction(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    // Subscribe to the scene loaded event when the script is enabled
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Unsubscribe from the event when the script is disabled/destroyed
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This function is called every time a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckCurrentSceneForDestruction(scene, mode);
    }

    private void CheckCurrentSceneForDestruction(Scene scene, LoadSceneMode mode)
    {
        // Check if the newly loaded scene is one of the forbidden scenes
        if (scene.name == "DIAG1" || scene.name == "Version2")
        {
            Debug.Log($"Scene loaded: {scene.name}. Destroying persistent object: {gameObject.name}");
            // Destroy the object if the current scene is a forbidden scene
            Destroy(gameObject);
        }
    }

    void OnMouseDown()
    {
        // OnMouseDown already handles the left mouse button press (0) by default.
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource not assigned to " + gameObject.name);
        }
    }
}