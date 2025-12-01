using UnityEngine;
using UnityEngine.Timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Quest System")]
    [SerializeField] private int _peopleMet = 0;
    [SerializeField] private bool _questActive = false;

    public int peopleMet => _peopleMet;
    public bool questActive 
    { 
        get => _questActive; 
        set => _questActive = value; 
    }

    [Header("NPC Memory")]
    [SerializeField] private Dictionary<string, string> _npcMemories = new();
    public Dictionary<string, string> npcMemories => _npcMemories;

    [Header("Current Character")]
    [SerializeField] private string _currentCharacter = "aya";
    [SerializeField] private string _currentIntimacyLevel = "Stranger";

    public string currentCharacter 
    { 
        get => _currentCharacter; 
        set => _currentCharacter = value; 
    }
    public string currentIntimacyLevel 
    { 
        get => _currentIntimacyLevel; 
        set => _currentIntimacyLevel = value; 
    }

    [Header("Intimacy Scores (0-100)")]
    [SerializeField] private int _ayaIntimacy = 0;
    [SerializeField] private int _mikaIntimacy = 0;
    [SerializeField] private int _soraIntimacy = 0;

    public int ayaIntimacy => _ayaIntimacy;
    public int mikaIntimacy => _mikaIntimacy;
    public int soraIntimacy => _soraIntimacy;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadIntimacyFromSave();
        }
        else Destroy(gameObject);
    }

    

    public void UpdateIntimacy(string personality, int newScore)
    {
        personality = personality.ToLower();
        switch (personality)
        {
            case "aya": _ayaIntimacy = Mathf.Clamp(newScore, 0, 100); break;
            case "mika": _mikaIntimacy = Mathf.Clamp(newScore, 0, 100); break;
            case "sora": _soraIntimacy = Mathf.Clamp(newScore, 0, 100); break;
        }

        if (currentCharacter == personality)
            currentIntimacyLevel = ScoreToLevel(newScore);

        Debug.Log($"[Intimacy Updated] {personality} -> Score: {newScore}, Level: {ScoreToLevel(newScore)}");

        if (newScore >= 100)
        {
            string sceneName = personality switch
            {
                "aya" => "AyaEnding3.1Cutscene",
                "mika" => "MikaEnding3.2Cutscene",
                "sora" => "SoraEnding3.3Cutscene",
                _ => ""
            };

            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.sceneLoaded += OnCutsceneSceneLoaded;
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("[GameManager] No cutscene mapped for personality: " + personality);
            }
        }
    }

    private void OnCutsceneSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe so it only runs once
        SceneManager.sceneLoaded -= OnCutsceneSceneLoaded;
        Debug.Log("[GameManager] Cutscene scene loaded. Starting timeline on scene-specific director...");

        StartCoroutine(ForcePlayTimeline(scene));
    }

    private IEnumerator ForcePlayTimeline(Scene cutsceneScene)
    {
        // give the scene a few frames to finish Awake/Start
        for (int i = 0; i < 3; i++)
            yield return new WaitForEndOfFrame();

        // find director only in the loaded scene
        PlayableDirector director = null;
        var roots = cutsceneScene.GetRootGameObjects();
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

        if (director == null)
        {
            Debug.LogError("[GameManager] No PlayableDirector found in the cutscene scene.");
            yield break;
        }

        Debug.Log($"[GameManager] Found scene-specific director '{director.gameObject.name}'. Forcing play (unscaled time)...");

        // ensure director's GameObject is active
        if (!director.gameObject.activeInHierarchy)
            director.gameObject.SetActive(true);

        // Use unscaled time so timeline plays even if timeScale == 0
        director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;

        // reset to start, evaluate and play
        director.time = 0;
        director.Evaluate();
        director.Play();

        // wait a few frames to let it start
        for (int f = 0; f < 5; f++)
            yield return null;

        // If it's not playing, attempt retries and manual time advancement as a last resort
        int attempts = 0;
        while (attempts < 120 && director.state != PlayState.Playing)
        {
            // advance director time manually using unscaled delta so we see progress
            director.time += Time.unscaledDeltaTime;
            director.Evaluate();
            director.Play(); // try to kick it into Playing state
            attempts++;
            yield return null;
        }

        Debug.Log($"[GameManager] Director final state: {director.state} (attempts={attempts})");
        if (director.state != PlayState.Playing)
            Debug.LogError("[GameManager] Timeline still not visibly playing. Check for scripts in the cutscene scene that might immediately deactivate or reset animated objects (SaveController, etc.), or check Time.timeScale and bindings.");
    }


    public int GetIntimacyScore(string personality)
    {
        return personality.ToLower() switch
        {
            "aya" => _ayaIntimacy,
            "mika" => _mikaIntimacy,
            "sora" => _soraIntimacy,
            _ => 0
        };
    }

    public string GetIntimacyLevel(string personality) => ScoreToLevel(GetIntimacyScore(personality));

    private string ScoreToLevel(int score)
    {
        if (score >= 80) return "Romantic Interest";
        if (score >= 60) return "Close Friend";
        if (score >= 40) return "Friend";
        if (score >= 20) return "Acquaintance";
        return "Stranger";
    }

    public void IncrementPeopleMet()
    {
        _peopleMet++;
        if (_peopleMet >= 3) _questActive = false;
    }

    public void TryStartMeetEveryoneQuest(string personality)
    {
        if (_peopleMet == 0)
        {
            _questActive = true;
            Debug.Log("[Quest Started] Meet Aya, Mika, and Sora!");
        }
    }

    private void SaveIntimacyToPlayerPrefs()
    {
        PlayerPrefs.SetInt("Intimacy_Aya", _ayaIntimacy);
        PlayerPrefs.SetInt("Intimacy_Mika", _mikaIntimacy);
        PlayerPrefs.SetInt("Intimacy_Sora", _soraIntimacy);
        PlayerPrefs.SetInt("PeopleMet", _peopleMet);
        PlayerPrefs.Save();
    }

    private void LoadIntimacyFromSave()
    {
        _ayaIntimacy = PlayerPrefs.GetInt("Intimacy_Aya", 0);
        _mikaIntimacy = PlayerPrefs.GetInt("Intimacy_Mika", 0);
        _soraIntimacy = PlayerPrefs.GetInt("Intimacy_Sora", 0);
        _peopleMet = PlayerPrefs.GetInt("PeopleMet", 0);
        _questActive = _peopleMet < 3;
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        _ayaIntimacy = _mikaIntimacy = _soraIntimacy = _peopleMet = 0;
        _questActive = true;
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}