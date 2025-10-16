using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // added for TMP usage

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public List<QuestProgress> activateQuests = new();
    private QuestUI questUI;
    // How often (seconds) to check for completed quests. Adjust as needed.
    private float checkInterval = 0.5f;
    private float checkTimer = 0f;

    // Popup prefab to show when a quest is accepted. Assign in inspector.
    public GameObject questAddedPopupPrefab;
    // Optional parent (assign your Canvas or a container). If null, instantiated at root.
    public Transform popupParent;

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

        // Show popup when a new quest is added
        SpawnQuestAddedPopup(quest.questName);
    }

    // Spawn the "Quest Added" popup using the assigned prefab.
    private void SpawnQuestAddedPopup(string questName)
    {
        if (questAddedPopupPrefab == null)
        {
            Debug.LogWarning("QuestController: questAddedPopupPrefab is not assigned. Cannot show quest popup.");
            return;
        }

        Debug.Log($"QuestController: Spawning quest popup for '{questName}' (parent: {(popupParent==null?"NULL":popupParent.name)})");

        // Instantiate under parent; use worldPositionStays = false so RectTransform anchors/local pos are preserved for UI prefabs.
        GameObject go = Instantiate(questAddedPopupPrefab, popupParent, false);

        if (go == null)
        {
            Debug.LogWarning("QuestController: Instantiate returned null.");
            return;
        }

        // Ensure RectTransform anchored position is sane (center top by default)
        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        var popup = go.GetComponent<QuestAddedPopup>();
        if (popup != null)
        {
            popup.Initialize($"Quest Added: {questName}");
        }
        else
        {
            // Fallback: set any TMP_Text found
            var tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp != null) tmp.text = $"Quest Added: {questName}";
            else Debug.LogWarning("QuestController: Spawned popup has no QuestAddedPopup or TMP_Text to set.");
        }
    }

    // Helper to test popup from the inspector/context menu
    [ContextMenu("Test Spawn Quest Popup")]
    private void TestSpawnQuestPopup()
    {
        SpawnQuestAddedPopup("TEST QUEST");
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

    // Called when a location (like School) is reached by the player. Marks matching ReachLocation objectives.
    public void OnLocationReached(string locationID)
    {
        if (QuestController.Instance == null) return;
        if (activateQuests == null || activateQuests.Count == 0) return;

        Debug.Log($"QuestController: Location reached: {locationID}. Checking active quests for ReachLocation objectives.");

        // For each active quest, mark any ReachLocation objectives that match this locationID
        foreach (var qp in activateQuests.ToArray())
        {
            bool modified = false;
            foreach (var obj in qp.objectives)
            {
                if (obj.type == ObjectiveType.ReachLocation)
                {
                    // Match either objectiveID or description to the provided locationID (trim, case-insensitive)
                    if (!string.IsNullOrEmpty(obj.objectiveID) && string.Equals(obj.objectiveID.Trim(), locationID.Trim(), System.StringComparison.OrdinalIgnoreCase)
                        || (!string.IsNullOrEmpty(obj.description) && string.Equals(obj.description.Trim(), locationID.Trim(), System.StringComparison.OrdinalIgnoreCase)))
                    {
                        obj.currentAmount = obj.requiredAmount;
                        modified = true;
                        Debug.Log($"QuestController: Marked ReachLocation objective '{obj.objectiveID ?? obj.description}' complete for quest '{qp.quest.questName}' (ID: {qp.QuestID}).");
                    }
                }
            }

            // If any objective changed, check if the quest is now completed
            if (modified && qp.IsCompleted)
            {
                Debug.Log($"QuestController: Quest '{qp.quest.questName}' completed by reaching '{locationID}'. Completing quest now.");
                CompleteQuest(qp.QuestID);
            }
            else if (modified)
            {
                // Update UI to reflect progress if partial
                questUI.UpdateQuestUI();
            }
        }
    }
}
