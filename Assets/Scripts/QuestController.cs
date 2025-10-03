using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public List<QuestProgress> activateQuests = new();
    private QuestUI questUI;
    // How often (seconds) to check for completed quests. Adjust as needed.
    private float checkInterval = 0.5f;
    private float checkTimer = 0f;

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
        Debug.Log($"QuestController: Accepted quest '{quest.questName}' (ID: {quest.questID}). Active quests: {activateQuests.Count}");
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
            Debug.Log($"QuestController: Completed quest '{quest.quest.questName}' (ID: {questID}). Active quests now: {activateQuests.Count}");
            questUI.UpdateQuestUI();
            return;
        }
        Debug.LogWarning($"QuestController: CompleteQuest called but no active quest found with ID {questID}");
    }

    private void Update()
    {
        // Throttle checks to a small interval instead of every frame
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            checkTimer = checkInterval;
            CheckForCompletedQuests();
        }
    }

    // Scan active quests and complete/remove any that have all objectives satisfied.
    private void CheckForCompletedQuests()
    {
        if (activateQuests == null || activateQuests.Count == 0) return;
        Debug.Log($"QuestController: Checking for completed quests. Active: {activateQuests.Count}");
        // collect completed quests first to avoid modifying collection while iterating
        var completed = activateQuests.FindAll(q => q.IsCompleted);
        foreach (var q in completed)
        {
            Debug.Log($"QuestController: Found completed quest '{q.quest.questName}' (ID: {q.QuestID}). Completing now.");
            // Use CompleteQuest to ensure any UI updates and consistent behavior
            CompleteQuest(q.QuestID);
        }
    }
}
