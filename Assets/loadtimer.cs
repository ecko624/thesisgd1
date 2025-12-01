using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class loadtimer : MonoBehaviour
{
    public float delayTime = 5f;
    public string nextSceneName = "NextScene";

    private PlayableDirector director;
    private Coroutine fallbackCoroutine;

    void Start()
    {
        StartCoroutine(InitAndStartDirector());
    }

    private IEnumerator InitAndStartDirector()
    {
        // wait a frame so Awake/Start in the loaded scene finish and bindings are set
        yield return new WaitForEndOfFrame();

        // prefer to find a PlayableDirector that's in the active scene (avoid persistent objects)
        Scene active = SceneManager.GetActiveScene();
        var roots = active.GetRootGameObjects();
        foreach (var root in roots)
        {
            var directors = root.GetComponentsInChildren<PlayableDirector>(true);
            foreach (var d in directors)
            {
                if (d != null && d.playableAsset != null)
                {
                    director = d;
                    break;
                }
            }
            if (director != null) break;
        }

        // fallback to global search only if scene-scoped search fails
        if (director == null)
            director = FindObjectOfType<PlayableDirector>();

        if (director != null && director.playableAsset != null)
        {
            // ensure director object is active
            if (!director.gameObject.activeInHierarchy)
                director.gameObject.SetActive(true);

            // robust to timeScale and race conditions
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;

            // reset and force play
            director.time = 0;
            director.Evaluate();
            director.Play();

            // subscribe to stopped and start fallback
            director.stopped += OnDirectorStopped;
            fallbackCoroutine = StartCoroutine(FallbackLoad());
            Debug.Log($"[loadtimer] Started director '{director.gameObject.name}' in scene '{active.name}' -> next='{nextSceneName}'");
        }
        else
        {
            // no director found in scene, fall back to timed load
            Debug.LogWarning($"[loadtimer] No scene PlayableDirector found in '{active.name}' — using timed load -> next='{nextSceneName}'");
            StartCoroutine(LoadNextScene());
        }
    }

    private void OnDirectorStopped(PlayableDirector d)
    {
        d.stopped -= OnDirectorStopped;
        if (fallbackCoroutine != null)
        {
            StopCoroutine(fallbackCoroutine);
            fallbackCoroutine = null;
        }
        Debug.Log($"[loadtimer] Director stopped — loading next scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(delayTime);
        Debug.Log($"[loadtimer] Timed load — loading next scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    // fallback: wait a bit longer than expected timeline and load if director didn't stop
    private IEnumerator FallbackLoad()
    {
        yield return new WaitForSeconds(delayTime + 1f);
        Debug.LogWarning($"[loadtimer] Fallback triggered — loading next scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (director != null)
            director.stopped -= OnDirectorStopped;
    }
}
