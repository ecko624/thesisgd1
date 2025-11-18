using System.Diagnostics;
using UnityEngine;

public class ServerLauncher : MonoBehaviour
{
    private Process serverProcess;

    void Start()
    {
        // Path to your server exe
        string serverExePath = "/Server/app.exe";
        UnityEngine.Debug.Log("Server path: " + serverExePath);

        if (!System.IO.File.Exists(serverExePath))
        {
            UnityEngine.Debug.LogError("Server executable not found!");
            return;
        }

        // Configure process
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = serverExePath;   // path to exe
        startInfo.UseShellExecute = true;     // <-- This is where you set it
        startInfo.CreateNoWindow = false;     // shows the console for debugging

        try
        {
            serverProcess = Process.Start(startInfo);
            UnityEngine.Debug.Log("Server started!");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Failed to start server: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (serverProcess != null && !serverProcess.HasExited)
        {
            serverProcess.Kill();
            UnityEngine.Debug.Log("Server stopped!");
        }
    }
}
