using UnityEngine;
using TMPro;

public class InputHandler : MonoBehaviour
{
    public DialogueManager dialogueManager;  // Assign in Inspector
    public TMP_InputField playerInputField;  // Assign in Inspector

    void Start()
    {
        if (playerInputField == null) return;

        // prefer onSubmit; also keep onEndEdit as a fallback
        playerInputField.onSubmit.RemoveListener(OnInputSubmit);
        playerInputField.onSubmit.AddListener(OnInputSubmit);

        playerInputField.onEndEdit.RemoveListener(OnInputSubmit);
        playerInputField.onEndEdit.AddListener(OnInputSubmit);
    }

    private void OnInputSubmit(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;
        if (dialogueManager == null)
        {
            Debug.LogWarning("InputHandler: dialogueManager not assigned.");
            return;
        }
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("InputHandler: GameManager.Instance is null.");
            return;
        }

        string personality = GameManager.Instance.currentCharacter;
        string tone = "cheerful";
        string intimacyLevel = GameManager.Instance.currentIntimacy;

        dialogueManager.GetNPCResponse(input.Trim(), personality, tone, intimacyLevel);

        playerInputField.text = "";
        playerInputField.ActivateInputField();
    }
}