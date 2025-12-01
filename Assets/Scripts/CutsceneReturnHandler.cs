using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class CutsceneReturnHandler : MonoBehaviour
{
    private void Start()
    {
        // Intentionally do NOT auto-load the next scene here. The cutscene scene
        // should allow the PlayableDirector to run to completion; when it stops,
        // CutsceneEndHandler will handle loading the next scene. Removing the
        // immediate load prevents the cutscene from being skipped.
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Restore player position
        float x = PlayerPrefs.GetFloat("PlayerX", player.transform.position.x);
        float y = PlayerPrefs.GetFloat("PlayerY", player.transform.position.y);
        float z = PlayerPrefs.GetFloat("PlayerZ", player.transform.position.z);
        player.transform.position = new Vector3(x, y, z);

        // Reattach camera
        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
            vcam.Follow = player.transform;

        // Refresh confiner
        CinemachineConfiner confiner = FindObjectOfType<CinemachineConfiner>();
        if (confiner != null)
            confiner.InvalidatePathCache();
    }
}
