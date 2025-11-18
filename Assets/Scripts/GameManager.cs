using UnityEngine;
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

            SceneManager.LoadScene(sceneName);
        }
    }

    private void OnCutsceneSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe so it only runs once
        SceneManager.sceneLoaded -= OnCutsceneSceneLoaded;

        PlayableDirector director = FindObjectOfType<PlayableDirector>();
        if (director != null)
        {
            Debug.Log("[GameManager] Found PlayableDirector, playing cutscene.");
            director.Play();
        }
        else
        {
            Debug.LogError("[GameManager] No PlayableDirector found in scene!");
        }
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