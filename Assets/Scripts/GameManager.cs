using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int peopleMet { get; private set; }
    public bool questActive { get; set; } // Public setter for external access
    public Dictionary<string, string> npcMemories { get; private set; }

    // Additional fields for character state
    public string currentCharacter { get; set; } // e.g., "outgoing_class_rep"
    public string currentTone { get; set; } // e.g., "cheerful"
    public string currentIntimacy { get; set; } // e.g., "strangers"

    // Intimacy levels for each NPC
    public int ayaIntimacy { get; private set; } = 0; // For "outgoing_class_rep"
    public int mikaIntimacy { get; private set; } = 0; // For "library_ghost"
    public int soraIntimacy { get; private set; } = 0; // For "free_spirit"

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            npcMemories = new Dictionary<string, string>();
            questActive = false; // Initialize quest state
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        // Initialize Act 1: Trigger grandfather interaction
        var dm = FindObjectOfType<DialogueControllerVersion2>();
        if (dm != null) dm.StartInteraction("grandfather");
    }

    public void UpdateIntimacy(string personality, int delta)
    {
        // Update intimacy based on personality
        switch (personality)
        {
            case "outgoing_class_rep":
                ayaIntimacy = Mathf.Clamp(ayaIntimacy + delta, -10, 10);
                currentIntimacy = GetIntimacyLevel(ayaIntimacy);
                break;
            case "library_ghost":
                mikaIntimacy = Mathf.Clamp(mikaIntimacy + delta, -10, 10);
                currentIntimacy = GetIntimacyLevel(mikaIntimacy);
                break;
            case "free_spirit":
                soraIntimacy = Mathf.Clamp(soraIntimacy + delta, -10, 10);
                currentIntimacy = GetIntimacyLevel(soraIntimacy);
                break;
        }
    }

    public void IncrementPeopleMet()
    {
        peopleMet++;
        questActive = peopleMet < 3; // Deactivate quest when complete
    }

    public string GetIntimacyLevel(int intimacy)
    {
        if (intimacy <= -3) return "strangers";
        if (intimacy <= 2) return "acquaintance";
        if (intimacy <= 5) return "friend";
        if (intimacy <= 8) return "close_friend";
        return "romantic_interest";
    }
}