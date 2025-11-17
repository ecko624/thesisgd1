using UnityEngine;

public class CloseDialogueButton : MonoBehaviour
{
    public DialogueControllerVersion2 dialogueController;

    public void CloseDialogue()
    {
        dialogueController.CloseDialoguePanel();
        dialogueController.ResetConversation();

        // Notify the active NPC
        NPCInteractable npc = FindObjectOfType<NPCInteractable>();
        if (npc != null)
            npc.EndConversation();
    }
}
