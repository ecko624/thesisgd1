using UnityEngine;
using System.Collections;
using Cinemachine;

public class SpawnManager : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(ReconnectAfterCutscene());
    }

    private IEnumerator ReconnectAfterCutscene()
    {
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠️ No player found in scene!");
            yield break;
        }

        // ✅ Restore player position after returning from cutscene
        if (PlayerPrefs.HasKey("PlayerX"))
        {
            float x = PlayerPrefs.GetFloat("PlayerX");
            float y = PlayerPrefs.GetFloat("PlayerY");
            float z = PlayerPrefs.GetFloat("PlayerZ");
            player.transform.position = new Vector3(x, y, z);
            Debug.Log($"✅ Player restored to position ({x}, {y}, {z}) after cutscene.");
        }

        // ✅ Reconnect Cinemachine camera
        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            vcam.Follow = player.transform;
            vcam.LookAt = player.transform;
        }

        // ✅ Refresh confiner
        CinemachineConfiner confiner = FindObjectOfType<CinemachineConfiner>();
        if (confiner != null)
        {
            confiner.InvalidatePathCache();
        }

        // Clear old data
        PlayerPrefs.DeleteKey("PlayerX");
        PlayerPrefs.DeleteKey("PlayerY");
        PlayerPrefs.DeleteKey("PlayerZ");
        PlayerPrefs.DeleteKey("NextSceneAfterCutscene");
        PlayerPrefs.Save();
    }
}
