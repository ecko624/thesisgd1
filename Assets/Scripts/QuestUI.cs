using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;

    // Start is called before the first frame update
    void Start()
    {
        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        Debug.Log("UpdateQuestUI called. Active quests: " + QuestController.Instance.activateQuests.Count);

        //Destroy existing quest entries
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        //Build quest entries
        foreach (var quest in QuestController.Instance.activateQuests) //If you want to test quest use testQuests instead of QuestController.Instance.activateQuests
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questNameText = entry.transform.Find("QuestNameText").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            string status = quest.IsCompleted ? " (Quest Complete)" : "";
            questNameText.text = quest.quest.name + status;

            foreach (var objective in quest.objectives)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount}";
            }
        }
    }
}
