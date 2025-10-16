using System;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class DialogueManager : MonoBehaviour
{
    public string serverUrl = "http://localhost:5000/generate";
    private string memoryContext = ""; 
    private const int maxContextLength = 200;

    // add this so other scripts can subscribe to model responses
    public event Action<string> OnModelResponse;

    // Identity (exposed so NPCInteractable and other systems can set them)
    [Header("Identity")]
    public string personality = "outgoing_class_rep";
    public string tone = "cheerful";
    public string playerId = "player1";
    public string npcId = "npc1";

    // UI
    [Header("UI")]
    public GameObject dialoguePanel;      // assign the DialoguePanel root here
    public TextMeshProUGUI npcText;       // assign DialogueText
    public TMP_InputField playerInputField; // optional: assign PlayerInput to disable while waiting

    [Header("Model Settings")]
    public int maxNewTokens = 120;
    public float temperature = 0.6f;

    // --- NEW: richer persona descriptions + few-shot examples ------------
    string GetPersonalityDescription(string personalityKey)
    {
        switch (personalityKey)
        {
            case "outgoing_class_rep":
                return "Outgoing class representative: warm, enthusiastic, uses friendly casual language, remembers campus events and social plans.";
            case "library_ghost":
                return "Quiet library regular: reserved, polite, uses soft tone, references books and quiet study habits.";
            case "free_spirit":
                return "Free-spirited friend: playful, spontaneous, uses colloquial phrasing and suggests fun activities.";
            default:
                return "A typical NPC with neutral, helpful conversational style.";
        }
    }

    string GetFewShotExamples()
    {
        // keep short to avoid blowing up context; adjust as needed
        var sb = new StringBuilder();
        sb.AppendLine("EXAMPLE:");
        sb.AppendLine("Player: Hey, want to hang out later?");
        sb.AppendLine("NPC: I’d love to — let’s meet after class by the quad. <|END|>");
        sb.AppendLine();
        sb.AppendLine("Player: Are you okay? You seem off.");
        sb.AppendLine("NPC: I’m a bit tired today, but I’ll be fine — thanks for asking. <|END|>");
        sb.AppendLine();
        sb.AppendLine("Player: Hi!");
        sb.AppendLine("NPC: Hey there — great to see you! How's your day going? <|END|>");
        sb.AppendLine("Player: How are you doing?");
        sb.AppendLine("NPC: I'm doing well — a little tired from studying but otherwise okay. <|END|>");
        sb.AppendLine();
        return sb.ToString();
    }

    // improved BuildPrompt that includes persona, memory and examples
    string BuildPrompt(string personality, string tone, string intimacyLabel, string memoryContext, string playerInput)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"SYSTEM: You are the NPC \"{personality}\".");
        sb.AppendLine($"CHARACTER: {GetPersonalityDescription(personality)}");
        sb.AppendLine($"STYLE: Tone=" + tone + ". Speak as this character. Use natural full sentences, reference past memory when helpful, and be specific (give brief details).");
        sb.AppendLine($"CONTEXT: intimacy={intimacyLabel}");
        sb.AppendLine("INSTRUCTION: If the player's message is a short greeting or similar, reply with a friendly, specific response instead of asking for clarification. Do NOT ask the player to clarify.");
        sb.AppendLine();
        if (!string.IsNullOrEmpty(memoryContext))
        {
            sb.AppendLine("Memory:");
            sb.AppendLine(memoryContext.Trim());
            sb.AppendLine();
        }
        sb.AppendLine(GetFewShotExamples());
        sb.AppendLine($"Player: {playerInput.Trim()}");
        sb.Append("NPC: ");

        return sb.ToString();
    }
    // -------------------------------------------------------------------

    // Call this to start a request; keeps UI responsive and shows "..." while the server is thinking
    public void GetNPCResponse(string playerInput, string personalityArg, string toneArg, string intimacyLevelArg)
    {
        // update local identity
        if (!string.IsNullOrEmpty(personalityArg)) personality = personalityArg;
        if (!string.IsNullOrEmpty(toneArg)) tone = toneArg;

        // build prompt
        string prompt = BuildPrompt(personality, tone, intimacyLevelArg, memoryContext, playerInput);

        // UI: show typing indicator and disable input while waiting
        if (npcText != null) npcText.text = "...";
        if (playerInputField != null) playerInputField.interactable = false;

        StartCoroutine(SendRequest(prompt, playerInput));
    }

    private IEnumerator SendRequest(string prompt, string playerInput)
    {
        // prepare JSON body (server may ignore extra keys, but include model settings if supported)
            var body = new RequestPayload
            {
                prompt = prompt,
                memory_context = memoryContext,
                player_input = playerInput,
                player_id = playerId,
                npc_id = npcId,
                max_new_tokens = maxNewTokens,
                temperature = temperature
            };
            string jsonData = JsonUtility.ToJson(body);
            Debug.Log("[DialogueManager] Request JSON: " + jsonData);   // <<-- add this line
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest www = new UnityWebRequest(serverUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
#if UNITY_2020_1_OR_NEWER
            www.timeout = 30;
#endif
            Debug.Log("[DialogueManager] Sending request: " + (prompt.Length > 200 ? prompt.Substring(0, 200) + "..." : prompt));
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool error = www.result != UnityWebRequest.Result.Success;
#else
            bool error = www.isNetworkError || www.isHttpError;
#endif
            if (error)
            {
                Debug.LogError("[DialogueManager] request error: " + www.error);
                if (npcText != null) npcText.text = "Sorry, I couldn't respond right now.";
                if (playerInputField != null) playerInputField.interactable = true;
                yield break;
            }

            string jsonResponse = www.downloadHandler.text;
            Debug.Log("[DialogueManager] Server raw response: " + jsonResponse);

            ResponseData data = null;
            try
            {
                data = JsonUtility.FromJson<ResponseData>(jsonResponse);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("DialogueManager: JSON parse failed: " + ex.Message);
            }

            string npcResponse = data != null && !string.IsNullOrEmpty(data.response) ? data.response : jsonResponse ?? "";

            // strip model end tokens for display
            string displayText = npcResponse;
            foreach (string variant in new[] { "<|END|>", "<|END||>", "<|END-|>", "<|END>" })
                displayText = displayText.Replace(variant, "");
            displayText = displayText.Trim();

            // Update UI and memory
            if (npcText != null)
            {
                npcText.text = displayText;
                Debug.Log("[DialogueManager] npcText set to: " + displayText);
            }

            // Update memory using the actual player input and model reply
            memoryContext += $"Player: {playerInput} NPC: {displayText} ";
            if (memoryContext.Length > maxContextLength)
                memoryContext = memoryContext.Substring(memoryContext.Length - maxContextLength);

            // notify subscribers
            OnModelResponse?.Invoke(displayText);

            // re-enable input
            if (playerInputField != null) playerInputField.interactable = true;

            // apply intimacy if provided
            if (data != null)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.UpdateIntimacy(personality, data.intimacy_delta);
                if (data.fallback)
                    Debug.Log("[DialogueManager] server returned fallback=true");
            }
        }
    }

    public void OpenDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            Debug.Log("[DialogueManager] Opened dialogue panel");
        }
        else
        {
            Debug.LogWarning("[DialogueManager] dialoguePanel is not assigned in the inspector");
        }
    }

    public void CloseDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            Debug.Log("[DialogueManager] Closed dialogue panel");
        }
    }

    // --- NEW: CloseConversation called by UI Close button -----------------
    public void CloseConversation()
    {
        // clear listeners so NPCs unsubscribe cleanly and no handlers remain
        OnModelResponse = null;

        // re-enable input and clear textbox if desired
        if (playerInputField != null)
        {
            playerInputField.interactable = true;
            playerInputField.text = "";
        }

        // hide panel
        CloseDialoguePanel();

        Debug.Log("[DialogueManager] CloseConversation: panel closed and listeners cleared.");
    }
    // -------------------------------------------------------------------
}

[System.Serializable]
public class RequestPayload
{
    public string prompt;
    public string memory_context;
    public string player_input;
    public string player_id;
    public string npc_id;
    public int max_new_tokens;
    public float temperature;
}

[System.Serializable]
public class ResponseData
{
    public string response;
    public bool fallback;
    public int intimacy_delta;
}