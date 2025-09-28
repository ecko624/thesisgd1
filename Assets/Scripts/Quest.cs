using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    TalkToNPC,
    // Add other quest types as needed
}

public enum ObjectiveType { CollectItem, DefeatEnemy, ReachLocation, TalkNPC, Custom }

[System.Serializable]
public class QuestObjective
{
    public string objectiveID;
    public string description;
    public ObjectiveType type;
    public int requiredAmount;
    public int currentAmount;
    public bool IsCompleted => currentAmount >= requiredAmount;
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest")]
public class Quest : ScriptableObject
{
    public string questID;
    public string questName;
    public string description;
    public List<QuestObjective> objectives;
    public QuestType questType;
    public string targetNPCName;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + System.Guid.NewGuid().ToString();
        }
    }
}

[System.Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<QuestObjective> objectives;

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjective>();
        if (quest.objectives != null)
        {
            foreach (var obj in quest.objectives)
            {
                objectives.Add(new QuestObjective
                {
                    objectiveID = obj.objectiveID,
                    description = obj.description,
                    type = obj.type,
                    requiredAmount = obj.requiredAmount,
                    currentAmount = 0
                });
            }
        }
    }

    public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted);
    public string QuestID => quest.questID;
}


