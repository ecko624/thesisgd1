using System.Collections.Generic;
using UnityEngine;

namespace Version2
{
    public class QuestControllerV2 : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject questUI;
        public Transform questListParent;
        public QuestEntryUI questEntryPrefab;

        [Header("NPC Settings")]
        [SerializeField] private List<string> npcNames = new List<string> { "Aya", "Mika", "Sora" };

        private GameManager gameManager;
        private bool wasOpen = false;  

        void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        void Update()
        {
            // Every time ESC is pressed → refresh quests
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UpdateQuestUI();
            }
        }

        void OnEnable()
        {
            UpdateQuestUI();
        }

        public void OpenQuestUI()
    {
        if (questUI != null)
        {
            questUI.SetActive(true);
            Canvas.ForceUpdateCanvases();  // refresh layout
        }

        UpdateQuestUI();
    }

        public void AddQuestEntry(string questTitle)
        {
            QuestEntryUI entry = Instantiate(questEntryPrefab, questListParent);
            entry.Setup(questTitle);
        }

        public void UpdateQuestUI()
        {
            if (gameManager == null) return;

            List<string> activeQuests = new List<string>();

            foreach (var npc in npcNames)
            {
                int score = gameManager.GetIntimacyScore(npc);

                if (score < 20)
                    activeQuests.Add($"Get to know {npc} (Stranger → Acquaintance)");
                else if (score < 40)
                    activeQuests.Add($"Build friendship with {npc} (Acquaintance → Friend)");
                else if (score < 60)
                    activeQuests.Add($"Deepen friendship with {npc} (Friend → Close Friend)");
                else if (score < 80)
                    activeQuests.Add($"Develop a close bond with {npc} (Close Friend → Romantic Interest)");
                else
                    activeQuests.Add($"Maintain your bond with {npc} (Romantic Interest)");
            }

            foreach (Transform child in questListParent)
                Destroy(child.gameObject);

            foreach (var questTitle in activeQuests)
                AddQuestEntry(questTitle);
        }
    }
}
