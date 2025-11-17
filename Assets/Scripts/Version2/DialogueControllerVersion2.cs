using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;

public class DialogueControllerVersion2 : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI npcText;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI intimacyDisplay;

    [Header("Server Settings")]
    [SerializeField] private string serverURL = "http://localhost:5000/generate";
    [SerializeField] private string playerID = "player1";

    public string currentPersonality = "aya";

    public delegate void ModelResponseHandler(string response);
    public event ModelResponseHandler OnModelResponse;

    [Serializable]
    private class ServerRequest { public string player_id; public string personality; public string player_input; }
    [Serializable]
    private class ServerResponse { public string response; public int intimacy_delta; public int intimacy_score; public string intimacy_level; }

    public TextMeshProUGUI NpcText => npcText;

    public void OpenDialoguePanel() => dialoguePanel.SetActive(true);
    public void CloseDialoguePanel() => dialoguePanel.SetActive(false);

    public void StartInteraction(string personality)
    {
        currentPersonality = personality.ToLower();
        dialoguePanel.SetActive(true);
        npcText.text = "";

        if (!GameManager.Instance.npcMemories.ContainsKey(currentPersonality))
            GetNPCResponse("Hey...");
    }

    public void GetNPCResponse(string playerInput)
    {
        if (string.IsNullOrWhiteSpace(playerInput)) return;
        playerInputField.interactable = false;
        playerInputField.text = "";
        StartCoroutine(SendToServer(playerInput.Trim()));
    }

    private IEnumerator SendToServer(string input)
    {
        var req = new ServerRequest
        {
            player_id = playerID,
            personality = currentPersonality,
            player_input = input
        };

        using var www = new UnityWebRequest(serverURL, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(req));
        www.uploadHandler = new UploadHandlerRaw(body);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            npcText.text = "I... can't reach you right now...";
            Debug.LogError(www.error);
        }
        else
        {
            var resp = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
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