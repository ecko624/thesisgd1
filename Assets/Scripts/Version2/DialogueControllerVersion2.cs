using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;

public class DialogueControllerVersion2 : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI npcText;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI intimacyDisplay;

    [Header("Server")]
    [SerializeField] private string serverURL = "http://localhost:5000/generate";
    [SerializeField] private string playerID = "player1";

    public string currentPersonality = "aya";

    public delegate void ModelResponseHandler(string response);
    public event ModelResponseHandler OnModelResponse;

    [Serializable] private class Request { public string player_id; public string personality; public string player_input; }
    [Serializable] private class Response { public string response; public int intimacy_delta; public int intimacy_score; public string intimacy_level; }

    public TextMeshProUGUI NpcText => npcText;

    public void OpenDialoguePanel() => dialoguePanel?.SetActive(true);
    public void CloseDialoguePanel() => dialoguePanel?.SetActive(false);

    public void StartInteraction(string personality)
{
    currentPersonality = personality.ToLower();

    // Only open panel and show greeting — DO NOT auto-send anything
    dialoguePanel?.SetActive(true);
    npcText.text = GetGreeting(personality);  // Just show greeting text, no server call

    // Optional: auto-focus input field
    if (playerInputField)
    {
        playerInputField.text = "";
        playerInputField.ActivateInputField();
    }
}

// Helper so each girl has her own greeting
private string GetGreeting(string personality)
{
    return personality switch
    {
        "aya" => "Hey! You came~ ♡",
        "mika" => "...Oh. It's you.",
        "sora" => "Yo! Took you long enough!",
        _ => "Hello there."
    };
}

    public void GetNPCResponse(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;
        playerInputField.interactable = false;
        playerInputField.text = "";
        StartCoroutine(Send(input.Trim()));
    }

    private IEnumerator Send(string input)
    {
        var req = new Request { player_id = playerID, personality = currentPersonality, player_input = input };
        using var www = new UnityWebRequest(serverURL, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(req));
        www.uploadHandler = new UploadHandlerRaw(body);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            npcText.text = "I... can't hear you...";
            Debug.LogError(www.error);
        }
        else
        {
            var resp = JsonUtility.FromJson<Response>(www.downloadHandler.text);
            npcText.text = resp.response;
            GameManager.Instance.UpdateIntimacy(currentPersonality, resp.intimacy_score);
            GameManager.Instance.currentIntimacyLevel = resp.intimacy_level;
            GameManager.Instance.IncrementPeopleMet();

            if (intimacyDisplay)
                intimacyDisplay.text = $"Heart {resp.intimacy_level} ({resp.intimacy_score}/100)";

            SaveMemory(input, resp.response);
            OnModelResponse?.Invoke(resp.response);
        }

        playerInputField.interactable = true;
        playerInputField.ActivateInputField();
    }

    private void SaveMemory(string input, string response)
    {
        var mem = GameManager.Instance.npcMemories;
        if (!mem.ContainsKey(currentPersonality)) mem[currentPersonality] = "";
        mem[currentPersonality] += $"You: {input}\n{currentPersonality}: {response}\n\n";
        if (mem[currentPersonality].Length > 1000)
            mem[currentPersonality] = mem[currentPersonality][^800..];
    }

    public void OnSubmitInput()
    {
        if (!string.IsNullOrWhiteSpace(playerInputField.text))
            GetNPCResponse(playerInputField.text);
    }
}