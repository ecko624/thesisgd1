using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneEndHandler : MonoBehaviour
{
    public PlayableDirector director;

    void Start()
    {
        // If not explicitly assigned, try to find any PlayableDirector in the scene
        if (director == null)
        {
            director = FindObjectOfType<PlayableDirector>();
            if (director != null)
            {
                Debug.Log("CutsceneEndHandler: Auto-assigned PlayableDirector: " + director.gameObject.name);
            }
            else
            {
                Debug.LogWarning("CutsceneEndHandler: No PlayableDirector found in scene for " + gameObject.name + ". The cutscene end will not be detected.");
                return;
            }
        }

        director.stopped += OnCutsceneEnd;
    }

    void OnCutsceneEnd(PlayableDirector pd)
    {
        string nextScene = PlayerPrefs.GetString("NextSceneAfterCutscene", "");
        if (!string.IsNullOrEmpty(nextScene))
        {
            Debug.Log("Cutscene finished. Loading scene from PlayerPrefs: " + nextScene);
            SceneManager.LoadScene(nextScene);
            return;
        }

        // Fallback: if the key wasn't set for any reason, load 'Version1'
        Debug.LogWarning("CutsceneEndHandler: PlayerPrefs key 'NextSceneAfterCutscene' was empty — loading fallback scene 'Version1'.");
        SceneManager.LoadScene("Version1");
    }

    void OnDestroy()
    {
        if (director != null)
            director.stopped -= OnCutsceneEnd;
    }
}
