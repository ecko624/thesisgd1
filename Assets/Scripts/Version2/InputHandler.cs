using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private DialogueControllerVersion2 dialogueController;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private Text quest2;
    public int quest = 10;

    void Start()
    {
        if (playerInputField)
            playerInputField.onEndEdit.AddListener(OnSubmit);
    }

    void Update()
    {
        if (quest2) quest2.text = quest + "/10";
        if (Input.GetKeyDown(KeyCode.Return) && playerInputField.isFocused)
            OnSubmit(playerInputField.text);
    }

    void OnSubmit(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        quest = Mathf.Max(0, quest - 1);
        if (quest <= 0)
        {
            string scene = SceneManager.GetActiveScene().name;
            if (scene.Contains("QUEST1")) SceneManager.LoadScene("QUEST1END");
            if (scene.Contains("QUEST2")) SceneManager.LoadScene("QUEST2END");
            if (scene.Contains("QUEST3")) SceneManager.LoadScene("QUEST3END");
        }

        dialogueController.GetNPCResponse(input.Trim());
        playerInputField.text = "";
        playerInputField.ActivateInputField();
    }

    public void OnSubmitButtonClick() => OnSubmit(playerInputField.text);
}