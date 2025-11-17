using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private DialogueControllerVersion2 dialogueController;
    [SerializeField] private TMP_InputField playerInputField;  // ← Make private + SerializeField
    [SerializeField] private Text quest2;                       // ← Your quest counter UI

    public int quest = 10;  // Your custom quest counter

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
        // Update quest display
        if (quest2 != null)
            quest2.text = quest + "/10";

        // Allow Enter key to submit (optional)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (playerInputField.isFocused && !string.IsNullOrWhiteSpace(playerInputField.text))
            {
                OnInputSubmit(playerInputField.text);
            }
        }
    }

    private void OnInputSubmit(string input)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // === YOUR CUSTOM QUEST LOGIC (unchanged) ===
        if (currentScene == "QUEST1GAMEPLAY" && quest <= 0)
        {
            SceneManager.LoadScene("QUEST1END");
            quest = 0;
            PlayerPrefs.SetInt("quest2", 1);
        }
        else if (currentScene == "QUEST21" && quest <= 0)
        {
            SceneManager.LoadScene("QUEST2END");
            quest = 0;
            PlayerPrefs.SetInt("quest3", 1);
        }
        else if (currentScene == "QUEST3GAMEPLAY" && quest <= 0)
        {
            SceneManager.LoadScene("QUEST3END");
            quest = 0;
            PlayerPrefs.SetInt("quest4", 1);
        }

        PlayerPrefs.Save();
        quest = Mathf.Max(0, quest - 1);  // Decrease safely

        if (string.IsNullOrWhiteSpace(input)) return;

        string trimmedInput = input.Trim();

        // === MODERN WAY: Let DialogueController handle personality & intimacy ===
        // No need to pass tone, intimacy level, or old names — server handles everything
        dialogueController.GetNPCResponse(trimmedInput);

        // Clear input
        playerInputField.text = "";
        playerInputField.ActivateInputField();
    }

    // For UI button (optional)
    public void OnSubmitButtonClick()
    {
        if (!string.IsNullOrWhiteSpace(playerInputField.text))
            OnInputSubmit(playerInputField.text);
    }
}