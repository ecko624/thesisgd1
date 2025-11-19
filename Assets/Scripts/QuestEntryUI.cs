using UnityEngine;
using TMPro;

public class QuestEntryUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;

    public void Setup(string title)
    {
        Debug.Log("Quest UI setup: " + title);
        titleText.text = title;
    }
}