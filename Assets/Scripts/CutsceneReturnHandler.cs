using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class CutsceneReturnHandler : MonoBehaviour
{
    private void Start()
    {
        string nextScene = PlayerPrefs.GetString("NextSceneAfterCutscene", "");
        if (string.IsNullOrEmpty(nextScene)) return;

        SceneManager.LoadScene(nextScene);
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
