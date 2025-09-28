using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public List<QuestProgress> activeQuests = new();
    private QuestUI questUI;
    public List<string> handinQuestIDs = new();
    [Header("Auto-Assign Quest")]
    public Quest autoAssignQuest; // Assign TalkToClassRep.asset in Inspector
    // Call this when the player talks to an NPC
    public void OnTalkToNPC(string npcName)
    {
        foreach (var questProgress in activeQuests)
        {
            var quest = questProgress.quest;
            if (quest != null && !questProgress.IsCompleted && quest.questType == QuestType.TalkToNPC && quest.targetNPCName == npcName)
            {
                // Mark all objectives as completed for this quest
                foreach (var obj in questProgress.objectives)
                {
                    obj.currentAmount = obj.requiredAmount;
                }
                questUI.UpdateQuestUI();
            }
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        questUI = FindObjectOfType<QuestUI>();

        // Automatically assign quest at game start if set
        if (autoAssignQuest != null)
        {
            AcceptQuest(autoAssignQuest);
        }
    }

    public void AcceptQuest(Quest quest)
    {
    if (IsQuestActive(quest.questID)) return;
    activeQuests.Add(new QuestProgress(quest));
    questUI.UpdateQuestUI();
    }

    public bool isQuestCompleted(string questID)
    {
    QuestProgress quest = activeQuests.Find(q => q.QuestID == questID);
    return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    }


    public bool IsQuestActive(string questID) => activeQuests.Exists(q => q.QuestID == questID);
}
