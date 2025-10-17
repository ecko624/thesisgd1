using System;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class DialogueControllerVersion2 : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI npcText;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private string serverUrl = "http://localhost:5000/generate";
    [SerializeField] public string personality = "grandfather";
    [SerializeField] public string tone = "shy";
    [SerializeField] private int maxNewTokens = 150; // Increased for longer responses
    [SerializeField] private float temperature = 0.75f;
    [SerializeField] private int maxContextLength = 200;
    [SerializeField] private string playerId = "player1";
    [SerializeField] private string npcId = "npc1";
    [SerializeField] private GameObject dialoguePanel;

    private string[] targetNPCs = { "outgoing_class_rep", "library_ghost", "free_spirit" };
    private bool questIntroduced = false; // Track if grandfather has given the intro

    public delegate void ModelResponseHandler(string response);
    public event ModelResponseHandler OnModelResponse;

    public string GetPlayerInputText()
    {
        return playerInputField != null ? playerInputField.text : "";
    }

    string BuildPrompt(string personality, string tone, string intimacyLevel, string memoryContext, string playerInput, bool isSingleLine = false)
    {
        string context = $"Setting: Hinode Bay, a coastal town. {GetPersonalityDescription(personality)}";
        string instruction = isSingleLine 
            ? $"Act as {personality} with a {tone} tone and {intimacyLevel} intimacy. Deliver a single, concise line of dialogue fitting Hinode Bay's story. Avoid further interaction prompts."
            : $"Act as {personality} with a {tone} tone and {intimacyLevel} intimacy. Use the memory to continue the conversation naturally, fitting Hinode Bay's story. Avoid repeating exact previous responses.";
        if (personality == "grandfather" && !questIntroduced)
        {
            instruction += " Provide a warm, multi-paragraph introduction to Hinode Bay, explaining its history and Ren's purpose. End with a quest to explore and meet three people: Aya, Elara, and Kai.";
        }
        return $"{context}\n{instruction}\nMemory: {memoryContext}\nPlayer: {playerInput}\nNPC:";
    }

    string GetPersonalityDescription(string personalityKey)
    {
        return personalityKey switch
        {
            "grandfather" => "A kind, slightly shy elder of Hinode Bay, guiding Ren with gentle advice. My name is Hiroshi.",
            "outgoing_class_rep" => "Hinode Bay's vibrant class rep, organizing beach events with a warm smile. My name is Aya.",
            "library_ghost" => "A quiet spirit haunting Hinode Bay's library, whispering about old tales. My name is Elara.",
            "free_spirit" => "A free-spirited surfer from Hinode Bay, eager for coastal adventures. My name is Kai.",
            _ => "A typical NPC in Hinode Bay."
        };
    }

    public void GetNPCResponse(string playerInput, string personalityArg = null, string toneArg = null, string intimacyLevelArg = null, bool isSingleLine = false)
    {
        if (!string.IsNullOrEmpty(playerInput) && playerInputField != null) playerInputField.text = "";
        if (playerInputField != null)
        {
            playerInputField.interactable = !isSingleLine; // Disable input for single-line NPCs
        }

        string selectedPersonality = personalityArg ?? personality;
        string selectedTone = toneArg ?? tone; // Use field value if no argument provided
        string intimacyLevel = intimacyLevelArg ?? "stranger";
        string memoryContext = GameManager.Instance.npcMemories.ContainsKey(selectedPersonality) ? GameManager.Instance.npcMemories[selectedPersonality] : "";
        string prompt = BuildPrompt(selectedPersonality, selectedTone, intimacyLevel, memoryContext, playerInput, isSingleLine);

        if (npcText != null) npcText.text = "...";
        StartCoroutine(SendRequest(prompt, playerInput, selectedPersonality, isSingleLine));
    }

    private IEnumerator SendRequest(string prompt, string playerInput, string selectedPersonality, bool isSingleLine)
    {
        var body = new RequestPayload
        {
            prompt = prompt,
            memory_context = GameManager.Instance.npcMemories.ContainsKey(selectedPersonality) ? GameManager.Instance.npcMemories[selectedPersonality] : "",
            player_input = playerInput,
            player_id = playerId,
            npc_id = npcId,
            max_new_tokens = maxNewTokens,
            temperature = temperature
        };
        string jsonData = JsonUtility.ToJson(body);
        Debug.Log("[DialogueManager] Request JSON: " + jsonData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest www = new UnityWebRequest(serverUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
#if UNITY_2020_1_OR_NEWER
            www.timeout = 30;
#endif
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[DialogueManager] request error: " + www.error);
                if (npcText != null) npcText.text = "Sorry, I couldn’t respond.";
                if (playerInputField != null) playerInputField.interactable = true;
                yield break;
            }

            string jsonResponse = www.downloadHandler.text;
            Debug.Log("[DialogueManager] Server raw response: " + jsonResponse);

            ResponseData data = JsonUtility.FromJson<ResponseData>(jsonResponse);
            string npcResponse = data?.response ?? "No response received.";
            int intimacyDelta = data?.intimacy_delta ?? 0;
            bool isFallback = data?.fallback ?? false;

            if (npcText != null)
            {
                npcText.text = npcResponse;
                Debug.Log("[DialogueManager] npcText set to: " + npcResponse);
            }

            if (selectedPersonality == "grandfather" && !questIntroduced)
            {
                questIntroduced = true;
                GameManager.Instance.questActive = true;
                npcText.text += "\n[Quest Started: Explore and Connect - Meet 3 people: Aya, Mika, and Sora]";
            }
            else if (GameManager.Instance.questActive && Array.IndexOf(targetNPCs, selectedPersonality) >= 0 && !GameManager.Instance.npcMemories.ContainsKey(selectedPersonality))
            {
                GameManager.Instance.IncrementPeopleMet();
                GameManager.Instance.npcMemories[selectedPersonality] = "";
                if (GameManager.Instance.peopleMet >= 3)
                {
                    npcText.text += "\n[Quest Complete: Explore and Connect]";
                    GameManager.Instance.questActive = false;
                    EnableAct2Interactions();
                }
                else
                {
                    npcText.text += $"\n[Met {GameManager.Instance.peopleMet}/3 people]";
                }
            }

            if (npcText != null && !string.IsNullOrEmpty(npcText.text))
            {
                string cleanResponse = npcText.text.Split('\n')[0];
                GameManager.Instance.npcMemories[selectedPersonality] = (GameManager.Instance.npcMemories.ContainsKey(selectedPersonality) ? GameManager.Instance.npcMemories[selectedPersonality] : "") +
                                                                      $"player_input: {playerInput} npc_response: {cleanResponse} ";
                if (GameManager.Instance.npcMemories[selectedPersonality].Length > maxContextLength)
                    GameManager.Instance.npcMemories[selectedPersonality] = GameManager.Instance.npcMemories[selectedPersonality].Substring(GameManager.Instance.npcMemories[selectedPersonality].Length - maxContextLength);
            }

            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.UpdateIntimacy(selectedPersonality, intimacyDelta);
            }

            OnModelResponse?.Invoke(npcResponse);
            if (playerInputField != null) playerInputField.interactable = !isSingleLine; // Re-enable if not single-line
        }
    }

    public void StartInteraction(string newPersonality, bool isSingleLine = false)
    {
        personality = newPersonality;
        GetNPCResponse("", personality, toneArg: tone, isSingleLine: isSingleLine); // Changed tone to toneArg
    }

    private void EnableAct2Interactions()
    {
        Debug.Log("Act 2 Interactions Enabled! Implement activity triggers here.");
    }

    public void OpenDialoguePanel() { if (dialoguePanel != null) dialoguePanel.SetActive(true); }
    public void CloseDialoguePanel() { if (dialoguePanel != null) dialoguePanel.SetActive(false); }
    public void CloseConversation()
    {
        OnModelResponse = null;
        if (playerInputField != null) { playerInputField.interactable = true; playerInputField.text = ""; }
        CloseDialoguePanel();
    }
}

[System.Serializable]
public class RequestPayload { public string prompt, memory_context, player_input, player_id, npc_id; public int max_new_tokens; public float temperature; }

[System.Serializable]
public class ResponseData { public string response; public bool fallback; public int intimacy_delta; }