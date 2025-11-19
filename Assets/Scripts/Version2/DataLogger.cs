using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataLogger : MonoBehaviour
{
    [SerializeField] private DialogueControllerVersion2 dialogueController;
    private RPGTalk rpgTalk;
    
    private string logPath;
    private string csvFilePath;
    private bool isInitialized = false;
    private bool hasNewPlayerChoice = false;
    private static DataLogger instance;

    private void Awake()
    {
        // Make this singleton persist across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("DataLogger created and set to persist across scenes");
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        // Subscribe to scene load event to find dialogue controller in new scenes
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Try to find dialogue controller on enable
        FindAndSubscribeToDialogueController();
    }

    private void OnDisable()
    {
        UnsubscribeFromDialogueController();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When a new scene loads, find the dialogue controller in that scene
        Debug.Log($"Scene loaded: {scene.name}. Looking for DialogueControllerVersion2...");
        FindAndSubscribeToDialogueController();
    }

    private void FindAndSubscribeToDialogueController()
    {
        // Unsubscribe from old controller first
        UnsubscribeFromDialogueController();
        // Find the DialogueControllerVersion2 in the scene
        DialogueControllerVersion2 controller = FindObjectOfType<DialogueControllerVersion2>();

        if (controller != null)
        {
            dialogueController = controller;
            dialogueController.OnModelResponse += LogInteraction;
            Debug.Log("DataLogger subscribed to DialogueControllerVersion2");
        }
        else
        {
            // Not an error: project may use RPGTalk instead
            dialogueController = null;
            Debug.Log("DialogueControllerVersion2 not found in current scene.");
        }

        // Also look for RPGTalk implementation and subscribe to its events
        RPGTalk found = FindObjectOfType<RPGTalk>();
        if (found != null)
        {
            rpgTalk = found;
            try
            {
                rpgTalk.OnMadeChoice += OnRPGMadeChoice;
            }
            catch (Exception) { }
            try
            {
                rpgTalk.OnPlayNext += OnRPGPlayNext;
            }
            catch (Exception) { }
            Debug.Log("DataLogger subscribed to RPGTalk events");
        }
        else
        {
            rpgTalk = null;
            Debug.Log("RPGTalk not found in current scene. Will search again when scene loads.");
        }
    }

    private void UnsubscribeFromDialogueController()
    {
        if (dialogueController != null)
        {
            dialogueController.OnModelResponse -= LogInteraction;
        }
        if (rpgTalk != null)
        {
            try { rpgTalk.OnMadeChoice -= OnRPGMadeChoice; } catch (Exception) { }
            try { rpgTalk.OnPlayNext -= OnRPGPlayNext; } catch (Exception) { }
            rpgTalk = null;
        }
    }

    private void Start()
    {
        InitializeLogger();
    }

    private void InitializeLogger()
    {
        if (isInitialized) return;

        // Set up the logging directory
        logPath = Path.Combine(Application.persistentDataPath, "DialogueLogs");
        Directory.CreateDirectory(logPath);

        csvFilePath = Path.Combine(logPath, "dialogue_log.csv");

        // Reset CSV only on first initialization (game start)
        ResetLog();

        isInitialized = true;
        Debug.Log($"Data Logger initialized. Log path: {csvFilePath}");
    }

    private void ResetLog()
    {
        try
        {
            // Clear existing log file
            if (File.Exists(csvFilePath))
            {
                File.Delete(csvFilePath);
            }

            // Create new CSV with headers (UTF-8 with BOM so Excel recognizes encoding)
            var bomUtf8 = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            using (StreamWriter writer = new StreamWriter(csvFilePath, false, bomUtf8))
            {
                writer.WriteLine("Timestamp,NPC,PlayerChoice,NPCResponse");
            }

            Debug.Log("Data log reset successfully!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error resetting log: {ex.Message}");
        }
    }

    public void LogInteraction(string npcResponse)
    {
        if (!isInitialized)
        {
            InitializeLogger();
        }

        try
        {
            // Determine NPC label
            string npc = "Unknown";
            if (dialogueController != null)
                npc = dialogueController.currentPersonality;
            else if (rpgTalk != null)
                npc = "RPGTalk";

            WriteLog(npc, npcResponse);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error logging interaction: {ex.Message}");
        }
    }

    private void WriteLog(string npc, string npcResponse)
    {
        if (!isInitialized)
            InitializeLogger();

        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string escapedResponse = (npcResponse ?? "").Replace("\"", "\"\"");
        string npcEscaped = (npc ?? "").Replace("\"", "\"\"");

        // Determine player choice: if no NEW choice since last log, record explicit text
        string playerChoiceRaw = hasNewPlayerChoice ? GetLastPlayerChoice() : "No player response";
        string playerChoiceEscaped = (playerChoiceRaw ?? "").Replace("\"", "\"\"");

        // Quote all fields to avoid CSV column issues and preserve commas/special chars
        string Quote(string s) => "\"" + s + "\"";
        string row = $"{timestamp},{Quote(npcEscaped)},{Quote(playerChoiceEscaped)},{Quote(escapedResponse)}";

        try
        {
            Debug.Log($"DataLogger: Writing CSV row -> {row}");
            // Use UTF8 without BOM for appending (file already has BOM if created by ResetLog)
            var utf8NoBom = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            using (StreamWriter writer = new StreamWriter(csvFilePath, true, utf8NoBom))
            {
                writer.WriteLine(row);
            }
            Debug.Log($"DataLogger: Successfully wrote entry to {csvFilePath}");

            // After writing, clear the new-choice flag so subsequent NPC lines without
            // a player action are logged as "No player response"
            hasNewPlayerChoice = false;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"DataLogger: Failed to write CSV row: {ex.Message}");
        }
    }

    private static string lastPlayerChoice = "";

    public static void UpdateLastPlayerChoice(string choice)
    {
        lastPlayerChoice = choice;
        if (instance != null)
            instance.hasNewPlayerChoice = true;
        try
        {
            Debug.Log($"DataLogger: Player choice updated -> {choice}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"DataLogger: Error logging player choice: {ex.Message}");
        }
    }

    // Handler invoked when RPGTalk reports a made choice
    private void OnRPGMadeChoice(string questionID, int choiceNumber)
    {
        string choiceText = $"{questionID}:{choiceNumber}";

        try
        {
            // Try reflection to obtain the actual choice text from RPGTalk's private questions list
            var qField = typeof(RPGTalk).GetField("questions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (qField != null && rpgTalk != null)
            {
                var qListObj = qField.GetValue(rpgTalk) as System.Collections.IList;
                if (qListObj != null)
                {
                    for (int i = 0; i < qListObj.Count; i++)
                    {
                        var q = qListObj[i];
                        var qidField = q.GetType().GetField("questionID");
                        if (qidField != null)
                        {
                            var qidVal = qidField.GetValue(q) as string;
                            if (qidVal == questionID)
                            {
                                var choicesField = q.GetType().GetField("choices");
                                if (choicesField != null)
                                {
                                    var choicesObj = choicesField.GetValue(q) as System.Collections.IList;
                                    if (choicesObj != null && choiceNumber >= 0 && choiceNumber < choicesObj.Count)
                                    {
                                        choiceText = choicesObj[choiceNumber]?.ToString() ?? choiceText;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"DataLogger: Reflection failed obtaining RPGTalk choice text: {ex.Message}");
        }

        // Update last player choice (this will also Debug.Log the update)
        UpdateLastPlayerChoice(choiceText);
        Debug.Log($"DataLogger: RPGTalk choice -> {questionID} #{choiceNumber} => {choiceText}");
    }

    // Handler invoked when RPGTalk advances to the next line
    private void OnRPGPlayNext()
    {
        if (rpgTalk == null) return;

        try
        {
            int pos = rpgTalk.cutscenePosition;
            if (pos <= 0) return;
            if (rpgTalk.rpgtalkElements != null && rpgTalk.rpgtalkElements.Count >= pos)
            {
                var elem = rpgTalk.rpgtalkElements[pos - 1];
                string speaker = elem.speakerName ?? "RPGTalk";
                string text = elem.dialogText ?? "";

                // Log the NPC response using the shared writer
                WriteLog(speaker, text);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"DataLogger: Error handling RPGTalk OnPlayNext: {ex.Message}");
        }
    }

    private string GetLastPlayerChoice()
    {
        return string.IsNullOrEmpty(lastPlayerChoice) ? "Unknown" : lastPlayerChoice.Replace("\"", "\"\"");
    }

    public string GetLogFilePath()
    {
        return csvFilePath;
    }

    public void OpenLogFolder()
    {
        if (Directory.Exists(logPath))
        {
            System.Diagnostics.Process.Start(logPath);
        }
    }
}
