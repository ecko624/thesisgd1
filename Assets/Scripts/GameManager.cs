using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // simple public state used by version2 scripts
    public string currentCharacter = "outgoing_class_rep";
    public string currentIntimacy = "stranger";

    // example raw intimacy counters per NPC (persist/save as needed)
    public int ayaIntimacy = 0;
    public int mikaIntimacy = 0;
    public int soraIntimacy = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Map raw intimacy to a label used in prompts
    public string GetIntimacyLevel(int raw)
    {
        if (raw <= 0) return "stranger";
        if (raw <= 3) return "acquaintance";
        if (raw <= 6) return "friend";
        return "close";
    }

    // Called by DialogueManager when server returns intimacy_delta
    public void UpdateIntimacy(string personality, int delta)
    {
        if (delta == 0) return;
        if (personality == "outgoing_class_rep") ayaIntimacy = Mathf.Clamp(ayaIntimacy + delta, -10, 99);
        else if (personality == "library_ghost") mikaIntimacy = Mathf.Clamp(mikaIntimacy + delta, -10, 99);
        else if (personality == "free_spirit") soraIntimacy = Mathf.Clamp(soraIntimacy + delta, -10, 99);
        // keep currentIntimacy in sync for convenience (optional)
        if (personality == currentCharacter)
        {
            int raw = (personality == "outgoing_class_rep") ? ayaIntimacy : (personality == "library_ghost" ? mikaIntimacy : soraIntimacy);
            currentIntimacy = GetIntimacyLevel(raw);
        }
    }
}