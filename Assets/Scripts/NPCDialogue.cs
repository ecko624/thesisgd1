using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCDialogue", menuName = "NPCDialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines; // Mark where dialogue ends
    public float autoProgressDelay = 1.5f; // Delay before automatically progressing to the next line
    public float typingSpeed = 0.05f; // Speed at which dialogue appears

    public DialogueChoice[] choices;

    public int questInProgressIndex; //What does he say while quest in progress
    public int questCompletedIndex; //What does he say when quest is completed
    public Quest quest; //Quest NPC gives
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex; //Dialogue line where choices appear
    public string[] choices; //Player response options
    public int[] nextDialogueIndexes; //Where choice leads
    public bool[] givesQuest; //If choice gives quest
}
