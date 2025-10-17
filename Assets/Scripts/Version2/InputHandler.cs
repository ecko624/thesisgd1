using UnityEngine;
using TMPro;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private DialogueControllerVersion2 dialogueController; // Assign in Inspector to DialogueManager GameObject
    public TMP_InputField playerInputField; // Assign in Inspector to the InputField in DialoguePanel

    void Start()
    {
        if (playerInputField != null)
        {
            playerInputField.onEndEdit.RemoveAllListeners();
            playerInputField.onEndEdit.AddListener(OnInputSubmit);
        }
        else
        {
            Debug.LogWarning("playerInputField not assigned in InputHandler!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && dialogueController != null)
        {
            string input = dialogueController.GetPlayerInputText(); // Use new method
            if (!string.IsNullOrEmpty(input))
            {
                dialogueController.GetNPCResponse(input); // Uses default personality
            }
        }
    }

    private void OnInputSubmit(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Trim().Length < 1) return;

        string trimmedInput = input.Trim();
        string personality = GameManager.Instance.currentCharacter; // e.g., "outgoing_class_rep"
        string tone = GameManager.Instance.currentTone;             // e.g., "cheerful"
        string intimacyLevel = GameManager.Instance.GetIntimacyLevel(
            personality == "outgoing_class_rep" ? GameManager.Instance.ayaIntimacy :
            personality == "library_ghost" ? GameManager.Instance.mikaIntimacy :
            GameManager.Instance.soraIntimacy
        );

        if (dialogueController != null)
        {
            dialogueController.GetNPCResponse(trimmedInput, personality, tone, intimacyLevel); // Updated call
        }

        playerInputField.text = "";
        playerInputField.ActivateInputField();
    }

    // Optional: Handle submit button if added to UI
    public void OnSubmitButtonClick()
    {
        OnInputSubmit(playerInputField.text);
    }
}