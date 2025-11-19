using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    public void PlayCutscene()
    {
        if (director != null)
            director.Play();
    }
}
