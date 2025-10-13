using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneEndHandler : MonoBehaviour
{
    public PlayableDirector director;

    void Start()
    {
        director.stopped += OnCutsceneEnd;
    }

    void OnCutsceneEnd(PlayableDirector pd)
    {
        string nextScene = PlayerPrefs.GetString("NextSceneAfterCutscene", "");
        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    void OnDestroy()
    {
        director.stopped -= OnCutsceneEnd;
    }
}
