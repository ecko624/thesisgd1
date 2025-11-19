using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Version2
{
public class QuestTabButton : MonoBehaviour
{
    public QuestControllerV2 questController;

    public void OpenQuestTab()
    {
        // activate UI tab
        // (whatever your tab logic already does)

        // refresh quests
        questController.UpdateQuestUI();
    }
}
}
