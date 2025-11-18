using UnityEngine;

public class PlayerPauseDuringDialogue : MonoBehaviour
{
    [SerializeField] private DialogueControllerVersion2 dialogueController;
    [SerializeField] private GameObject dialoguePanel;  // Drag your DialoguePanel here
    [SerializeField] private MonoBehaviour[] movementScripts; // Drag your player movement scripts here (e.g. CharacterController2D, PlayerMovement, etc.)

    private void Update()
    {
        bool dialogueOpen = dialoguePanel.activeInHierarchy;

        // Disable movement scripts
        foreach (var script in movementScripts)
            if (script != null) script.enabled = !dialogueOpen;

        // Optional: pause any time / day-night system
        Time.timeScale = dialogueOpen ? 0f : 1f;
    }
}