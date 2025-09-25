using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive; 

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
            return; // Exit if no dialogue data or game is paused
        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        PauseController.SetPause(true);

        DisplayCurrentLine();
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }

    //Clear Choices
    Debug.Log("Clearing choices");

        // Increment dialogueIndex and display the line
        dialogueIndex++;
        if (dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }

        // Choices and end checks will be handled after TypeLine finishes
    }

    IEnumerator TypeLine()
    {
    Debug.Log($"TypeLine coroutine started for index {dialogueIndex}");
    isTyping = true;
        dialogueUI.SetDialogueText("");
        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text += letter);
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;
        // After typing, check for endDialogueLines
        if (dialogueData.endDialogueLines.Length > dialogueIndex && dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            yield break;
        }
        // After typing, check for choices
        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            Debug.Log($"Checking choice: dialogueChoice.dialogueIndex={dialogueChoice.dialogueIndex}, current dialogueIndex={dialogueIndex}");
            if (dialogueChoice.dialogueIndex == dialogueIndex)
            {
                Debug.Log("entered forLoop if statement display choices");
                DisplayChoices(dialogueChoice);
                yield break;
            }
        }
        // Handle auto-progress
        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            Debug.Log($"Auto-progress triggered for index {dialogueIndex}");
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        Debug.Log("Displaying choices");
        dialogueUI.ClearChoices();
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex));
            Debug.Log("Choice displayed: " + choice.choices[i]);
        }
        Debug.Log("All choices should be displayed by now");
    }

    void ChooseOption(int nextIndex)
    {
    dialogueIndex = nextIndex;
    Debug.Log($"ChooseOption called. dialogueIndex set to: {dialogueIndex}");
    dialogueUI.ClearChoices();
    DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }
    
    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);

    }
}
