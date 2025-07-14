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
    public float autoProgressDelay = 1.5f; // Delay before automatically progressing to the next line
    public float typingSpeed = 0.05f; // Speed at which dialogue appears
}
