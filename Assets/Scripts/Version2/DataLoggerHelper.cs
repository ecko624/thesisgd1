using UnityEngine;

/// <summary>
/// Helper script for accessing and managing the DataLogger
/// Attach this to a GameObject in your main scene or use it as a reference
/// </summary>
public class DataLoggerHelper : MonoBehaviour
{
    private DataLogger dataLogger;

    private void Start()
    {
        // Find the DataLogger in the scene
        dataLogger = FindObjectOfType<DataLogger>();
        
        if (dataLogger == null)
        {
            Debug.LogError("DataLogger not found in scene! Make sure it's attached to a GameObject.");
        }
    }

    /// <summary>
    /// Opens the folder containing the dialogue log CSV
    /// </summary>
    public void OpenDialogueLogFolder()
    {
        if (dataLogger != null)
        {
            dataLogger.OpenLogFolder();
            Debug.Log("Opened dialogue log folder");
        }
    }

    /// <summary>
    /// Gets the full path to the CSV log file
    /// </summary>
    public string GetLogPath()
    {
        if (dataLogger != null)
        {
            return dataLogger.GetLogFilePath();
        }
        return "";
    }

    /// <summary>
    /// Debug method - call from console to open logs
    /// Usage: FindObjectOfType<DataLoggerHelper>().OpenDialogueLogFolder();
    /// </summary>
    [ContextMenu("Open Log Folder")]
    public void DebugOpenLogFolder()
    {
        OpenDialogueLogFolder();
    }
}
