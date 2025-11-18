using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerPrefsCSVExporter : MonoBehaviour
{
    private const string PLAYER_PREFS_KEY = "DialogueLog";

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

    // Export PlayerPrefs logs to CSV
    public void ExportPlayerPrefsToCSV()
    {
        if (!PlayerPrefs.HasKey(PLAYER_PREFS_KEY))
        {
            Debug.LogWarning("No conversation logs found in PlayerPrefs.");
            return;
        }

        string json = PlayerPrefs.GetString(PLAYER_PREFS_KEY);
        ConversationListWrapper wrapper = JsonUtility.FromJson<ConversationListWrapper>(json);
        List<ConversationEntry> conversationLog = wrapper.conversations;

        if (conversationLog.Count == 0)
        {
            Debug.LogWarning("No conversation logs to export.");
            return;
        }

        // Create folder for CSV
        string folderPath = Application.persistentDataPath + "/ConversationLogs";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = folderPath + "/conversation_log2.csv";

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // CSV header
            writer.WriteLine("Timestamp,NPC Name,Player Response,NPC Response");

            foreach (var entry in conversationLog)
            {
                string line = $"\"{entry.timestamp}\",\"{entry.npcName}\",\"{entry.playerResponse}\",\"{entry.npcResponse}\"";
                writer.WriteLine(line);
            }
        }

        Debug.Log($"PlayerPrefs conversation logs exported to CSV: {filePath}");
    }
}
