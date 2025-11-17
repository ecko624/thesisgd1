using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueControllerVersion2 : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] public TextMeshProUGUI npcText;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private GameObject dialoguePanel;

    [Header("NPC Defaults")]
    [SerializeField] public string personality = "aya";
    [SerializeField] public string tone = "cheerful";
    [SerializeField] private string intimacy = "stranger";

    [Header("Context Memory Settings")]
    [SerializeField] private int maxContextLength = 200;

    private bool questIntroduced = false;
    private string[] targetNPCs = { "outgoing_class_rep", "library_ghost", "free_spirit" };

    private Dictionary<string, List<DialogueRow>> dialogueDB = new Dictionary<string, List<DialogueRow>>();

    public delegate void ModelResponseHandler(string response);
    public event ModelResponseHandler OnModelResponse;

    // ------------------------
    // PlayerPrefs logging
    // ------------------------
    [Serializable]
    public class ConversationEntry
    {
        public string npcName;
        public string playerResponse;
        public string npcResponse;
        public string timestamp;
    }

    [Serializable]
    public class ConversationListWrapper
    {
        public List<ConversationEntry> conversations = new List<ConversationEntry>();
    }

    private List<ConversationEntry> conversationLog = new List<ConversationEntry>();
    private const string PLAYER_PREFS_KEY = "DialogueLog";

    // ------------------------
    // STRUCT FOR CSV ROWS
    // ------------------------
    [Serializable]
    public class DialogueRow
    {
        public string personality;
        public string tone;
        public string intimacy;
        public string playerInput;
        public string response;
        public int intimacyDelta;
    }

    private void Awake()
    {
        LoadCSV();
        LoadConversationLog();
    }

    private void LoadCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("dialogue");

        if (csvFile == null)
        {
            Debug.LogError("dialogue.csv NOT FOUND! Place it under Assets/Resources/");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("personality")) continue;

            string[] c = line.Split(',');
            if (c.Length < 6) continue;

            DialogueRow row = new DialogueRow
            {
                personality = c[0].Trim().ToLower(),
                tone = c[1].Trim(),
                intimacy = c[2].Trim(),
                playerInput = c[3].Trim(),
                response = c[4].Trim(),
                intimacyDelta = int.TryParse(c[5].Trim(), out int val) ? val : 0
            };

            string key = row.personality;

            if (!dialogueDB.ContainsKey(key))
                dialogueDB[key] = new List<DialogueRow>();

            dialogueDB[key].Add(row);
        }

        Debug.Log("CSV Loaded Successfully.");
    }

    private void LoadConversationLog()
    {
        if (PlayerPrefs.HasKey(PLAYER_PREFS_KEY))
        {
            string json = PlayerPrefs.GetString(PLAYER_PREFS_KEY);
            ConversationListWrapper wrapper = JsonUtility.FromJson<ConversationListWrapper>(json);
            conversationLog = wrapper.conversations;
            Debug.Log($"Loaded {conversationLog.Count} conversations from PlayerPrefs.");
        }
    }

    public void GetNPCResponse(string playerInput, string personalityArg = null, string toneArg = null, string intimacyArg = null, bool isSingleLine = false)
    {
        if (playerInputField != null)
        {
            playerInputField.text = "";
            playerInputField.interactable = !isSingleLine;
        }

        personality = (personalityArg ?? personality).ToLower();
        tone = toneArg ?? tone;
        intimacy = intimacyArg ?? intimacy;

        string npcResponse = FindBestDialogue(personality, tone, intimacy, playerInput, out int delta);

        npcText.text = npcResponse;

        HandleQuestLogic(personality);

        SaveMemory(personality, playerInput, npcResponse);

        GameManager.Instance.UpdateIntimacy(personality, delta);

        OnModelResponse?.Invoke(npcResponse);
    }

    private string FindBestDialogue(string personality, string tone, string intimacy, string playerInput, out int delta)
    {
        delta = 0;

        if (!dialogueDB.ContainsKey(personality))
            return "…I don’t seem to have anything to say right now.";

        List<DialogueRow> list = dialogueDB[personality];

        float similarityThreshold = 0.7f;

        foreach (var row in list)
        {
            if (!string.IsNullOrEmpty(row.playerInput))
            {
                string[] keywords = row.playerInput.Split('|');
                foreach (var kw in keywords)
                {
                    float similarity = GetSimilarity(playerInput, kw.Trim());
                    if (similarity >= similarityThreshold)
                    {
                        delta = row.intimacyDelta;
                        return row.response;
                    }
                }
            }
        }

        foreach (var row in list)
        {
            if (string.IsNullOrEmpty(row.playerInput))
            {
                delta = row.intimacyDelta;
                return row.response;
            }
        }

        return "Hmm… I’m not sure what to say.";
    }

    private void HandleQuestLogic(string npc)
    {
        GameManager gm = GameManager.Instance;

        if (npc == "grandfather" && !questIntroduced)
        {
            questIntroduced = true;
            gm.questActive = true;
            npcText.text += "\n[Quest Started: Explore and Connect — Meet Aya, Elara, and Kai]";
        }
        else if (gm.questActive && Array.IndexOf(targetNPCs, npc) >= 0 && !gm.npcMemories.ContainsKey(npc))
        {
            gm.IncrementPeopleMet();
            gm.npcMemories[npc] = "";

            if (gm.peopleMet >= 3)
            {
                npcText.text += "\n[Quest Complete: Explore and Connect]";
                gm.questActive = false;
            }
            else
            {
                npcText.text += $"\n[Met {gm.peopleMet}/3 people]";
            }
        }
    }

    private void SaveMemory(string personality, string playerInput, string npcResponse)
    {
        GameManager gm = GameManager.Instance;

        if (!gm.npcMemories.ContainsKey(personality))
            gm.npcMemories[personality] = "";

        gm.npcMemories[personality] += $"player_input:{playerInput} npc_response:{npcResponse} ";

        if (gm.npcMemories[personality].Length > maxContextLength)
        {
            int len = gm.npcMemories[personality].Length;
            gm.npcMemories[personality] =
                gm.npcMemories[personality].Substring(len - maxContextLength);
        }

        // ------------------------
        // Save to PlayerPrefs
        // ------------------------
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        ConversationEntry entry = new ConversationEntry
        {
            npcName = personality,
            playerResponse = playerInput,
            npcResponse = npcResponse,
            timestamp = timestamp
        };

        conversationLog.Add(entry);

        string json = JsonUtility.ToJson(new ConversationListWrapper { conversations = conversationLog });
        PlayerPrefs.SetString(PLAYER_PREFS_KEY, json);
        PlayerPrefs.Save();

        Debug.Log($"Saved conversation in PlayerPrefs: {timestamp} - {personality}");
    }

    public void OpenDialoguePanel() => dialoguePanel?.SetActive(true);
    public void CloseDialoguePanel() => dialoguePanel?.SetActive(false);

    public void StartInteraction(string newPersonality, bool isSingleLine = false)
    {
        personality = newPersonality.ToLower();
        GetNPCResponse("", newPersonality, tone, intimacy, isSingleLine);
    }

    public void CloseConversation()
    {
        OnModelResponse = null;
        npcText.text = "";

        if (playerInputField != null)
        {
            playerInputField.interactable = true;
            playerInputField.text = "";
        }

        CloseDialoguePanel();
    }

    private float GetSimilarity(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return 0f;

        int distance = LevenshteinDistance(a.ToLower(), b.ToLower());
        int maxLen = Math.Max(a.Length, b.Length);
        return 1f - ((float)distance / maxLen);
    }

    private int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }

        return d[n, m];
    }
}
