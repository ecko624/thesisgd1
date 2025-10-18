using UnityEngine;

namespace Version2
{
    public class QuestControllerV2 : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject questUI;

        void Awake()
        {
            if (questUI == null)
            {
                var found = GameObject.Find("QuestUI");
                if (found != null)
                {
                    questUI = found;
                    Debug.Log("QuestControllerV2: auto-found QuestUI");
                }
                else
                {
                    Debug.LogWarning("QuestControllerV2 Awake: questUI is NULL — assign in inspector or create a UI GameObject named 'QuestUI'");
                }
            }
        }

        public void OpenQuestUI()
        {
            if (questUI == null) { Debug.LogWarning("OpenQuestUI called but questUI is null."); return; }
            questUI.SetActive(true);
        }

        public void CloseQuestUI()
        {
            if (questUI == null) return;
            questUI.SetActive(false);
        }
    }
}