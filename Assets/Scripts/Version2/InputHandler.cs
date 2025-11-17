using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private DialogueControllerVersion2 dialogueController;
    public TMP_InputField playerInputField;
    public int quest = 10;
    public Text quest2;

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
        quest2.text = quest.ToString() + "/10";

        // ENTER key submit
        if (Input.GetKeyDown(KeyCode.Return) && dialogueController != null)
        {
            string input = playerInputField.text;  // ✅ FIXED: read directly from input field

            if (!string.IsNullOrEmpty(input))
            {
                dialogueController.GetNPCResponse(input);
            }
        }
    }

    private void OnInputSubmit(string input)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // === QUEST LOGIC ===
        if (currentScene == "QUEST1GAMEPLAY")
        {
            if (quest <= 0)
            {
                Application.LoadLevel("QUEST1END");
                quest = 0;
            }
            PlayerPrefs.SetInt("quest2", 1);
            PlayerPrefs.Save();
        }
        if (currentScene == "QUEST21")
        {
            if (quest <= 0)
            {
                Application.LoadLevel("QUEST2END");
                quest = 0;
            }
            PlayerPrefs.SetInt("quest3", 1);
            PlayerPrefs.Save();
        }
        if (currentScene == "QUEST3GAMEPLAY")
        {
            if (quest <= 0)
            {
                Application.LoadLevel("QUEST3END");
                quest = 0;
            }
            PlayerPrefs.SetInt("quest4", 1);
            PlayerPrefs.Save();
        }

        quest -= 1;

        if (string.IsNullOrWhiteSpace(input) || input.Trim().Length < 1) return;

        string trimmedInput = input.Trim();

        string personality = GameManager.Instance.currentCharacter;
        string tone = GameManager.Instance.currentTone;

        string intimacyLevel = GameManager.Instance.GetIntimacyLevel(
            personality == "outgoing_class_rep" ? GameManager.Instance.ayaIntimacy :
            personality == "library_ghost" ? GameManager.Instance.mikaIntimacy :
            GameManager.Instance.soraIntimacy
        );

        // Send to dialogue controller
        if (dialogueController != null)
        {
            dialogueController.GetNPCResponse(trimmedInput, personality, tone, intimacyLevel);
        }

        // Reset input
        playerInputField.text = "";
        playerInputField.ActivateInputField();
    }

    public void OnSubmitButtonClick()
    {
        OnInputSubmit(playerInputField.text);
    }
}
