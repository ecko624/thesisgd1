using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public List<QuestProgress> activateQuests = new();
    private QuestUI questUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        questUI = FindObjectOfType<QuestUI>();
        Debug.Log("QuestController Awake: questUI is " + (questUI == null ? "NULL" : "FOUND"));
    }

    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID)) return;
        
        activateQuests.Add(new QuestProgress(quest));

        questUI.UpdateQuestUI();
    }

    public bool IsQuestActive(string questID) => activateQuests.Exists(q => q.QuestID == questID);

    public void CompleteQuest(string questID)
    {
        var quest = activateQuests.Find(q => q.QuestID == questID);
        if (quest != null)
        {
            foreach (var obj in quest.objectives)
            {
                obj.currentAmount = obj.requiredAmount;
            }
            // Remove quest from log after completion
            activateQuests.Remove(quest);
            questUI.UpdateQuestUI();
        }
    }
}
