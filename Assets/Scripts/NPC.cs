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

    private enum QuestState { NotStarted, InProgress, Completed}
    private QuestState questState = QuestState.NotStarted;

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

    private void SyncQuestState()
    {
        if (dialogueData.quest == null) return;

        string questID = dialogueData.quest.questID;

        //Future update add completing quest and handing in
        if (QuestController.Instance.IsQuestActive(questID))
        {
            questState = QuestState.InProgress;
        }
        else
        {
            questState = QuestState.NotStarted;
        }
    }

    void StartDialogue()
    {
        //Sync with quest data
        SyncQuestState();

        //Set dialogue line based on questState
        if (questState == QuestState.NotStarted)
        {
            dialogueIndex = 0;
        }
        else if (questState == QuestState.InProgress)
        {
            dialogueIndex = dialogueData.questInProgressIndex;
        }
        else if (questState == QuestState.Completed)
        {
            dialogueIndex = dialogueData.questCompletedIndex;
        }

        isDialogueActive = true;

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
        // Safety: Use the smallest array length to avoid IndexOutOfRangeException
        int count = Mathf.Min(choice.choices.Length, choice.nextDialogueIndexes.Length, choice.givesQuest.Length);
        for (int i = 0; i < count; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            bool givesQuest = choice.givesQuest[i];
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex, givesQuest));
            Debug.Log("Choice displayed: " + choice.choices[i]);
        }
        if (choice.choices.Length != count || choice.nextDialogueIndexes.Length != count || choice.givesQuest.Length != count)
        {
            Debug.LogError("DialogueChoice arrays are not the same length!");
        }
        Debug.Log("All choices should be displayed by now");
    }

    void ChooseOption(int nextIndex, bool givesQuest)
    {
        if (givesQuest)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }

        // Complete the quest if this NPC is the quest target
        if (dialogueData.quest != null && questState == QuestState.InProgress
            && dialogueData.quest.targetNPCName == dialogueData.npcName)
        {
            QuestController.Instance.CompleteQuest(dialogueData.quest.questID);
            questState = QuestState.Completed;
        }

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
