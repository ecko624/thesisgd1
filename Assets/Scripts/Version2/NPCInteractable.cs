using UnityEngine;
using UnityEngine.Events;

// Ensure this implements the project's IInteractable so InteractionDetector can call it directly,
// and optionally create a child trigger + InteractionForwarder so the Version1 trigger-based detector still works.
namespace Version2
{
    public class NPCInteractable : MonoBehaviour, IInteractable
    {
        [Header("Identity")]
        public string personalityKey = "outgoing_class_rep";
        public string npcId = "aya";
        public Sprite portrait;
        [TextArea] public string greeting = "Hey!";
        public string tone = "cheerful";

        [Header("Interaction")]
        public GameObject interactPrompt;      // popup object (world-space or UI)
        public KeyCode interactionKey = KeyCode.E;
        public float maxInteractDistance = 2f; // used when not using triggers
        public bool useTrigger = false;        // prefer distance-based by default
        [Tooltip("If true, a child trigger collider + InteractionForwarder will be added so Version1's InteractionDetector works without making the main collider a trigger.")]
        public bool addTriggerChildForDetection = true;
        public Vector2 triggerChildSize = new Vector2(1.5f, 2f);

        [Header("Events")]
        public UnityEvent onOpenDialogue;
        public UnityEvent onCloseDialogue;

        DialogueManager dm;
        bool playerInRange = false;
        Transform playerTransform;
        bool isDialogueActive = false;

        void Awake()
        {
            dm = FindObjectOfType<DialogueManager>();
            if (dm == null) Debug.LogWarning("NPCInteractable: DialogueManager not found in scene.");
            if (interactPrompt != null) interactPrompt.SetActive(false);

            if (addTriggerChildForDetection)
                EnsureTriggerChildExists();
        }

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        void Update()
        {
            if (!useTrigger)
            {
                UpdateDistanceCheck();
                if (!isDialogueActive && playerInRange && Input.GetKeyDown(interactionKey))
                    TryInteract();
            }
            else
            {
                if (!isDialogueActive && playerInRange && Input.GetKeyDown(interactionKey))
                    TryInteract();
            }
        }

        void UpdateDistanceCheck()
        {
            if (playerTransform == null)
            {
                playerInRange = false;
                SetPromptActive(false);
                return;
            }

            float d = Vector2.Distance(transform.position, playerTransform.position);
            bool nowInRange = d <= maxInteractDistance;
            if (nowInRange != playerInRange)
            {
                playerInRange = nowInRange;
                SetPromptActive(playerInRange);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!useTrigger) return;
            if (!other.CompareTag("Player")) return;
            playerInRange = true;
            SetPromptActive(true);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!useTrigger) return;
            if (!other.CompareTag("Player")) return;
            playerInRange = false;
            SetPromptActive(false);
        }

        void SetPromptActive(bool active)
        {
            if (interactPrompt != null) interactPrompt.SetActive(active);
        }

        // IInteractable API for InteractionDetector
        public void Interact()
        {
            if (!CanInteract()) return;
            TryInteract();
        }

        public bool CanInteract()
        {
            return !isDialogueActive && playerInRange;
        }

        void TryInteract()
        {
            if (dm == null) dm = FindObjectOfType<DialogueManager>();
            if (dm == null) return;

            isDialogueActive = true;

            // set identity for DialogueManager and GameManager
            dm.personality = personalityKey;
            dm.npcId = npcId;
            dm.tone = tone;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentCharacter = personalityKey;
                // sync label
                GameManager.Instance.currentIntimacy = GameManager.Instance.GetIntimacyLevel(
                    personalityKey == "outgoing_class_rep" ? GameManager.Instance.ayaIntimacy :
                    personalityKey == "library_ghost" ? GameManager.Instance.mikaIntimacy : GameManager.Instance.soraIntimacy
                );
            }

            if (dm.npcText != null) dm.npcText.text = greeting;

            // explicitly open the UI panel
            dm.OpenDialoguePanel();

            // invoke events and subscribe
            onOpenDialogue?.Invoke();
            dm.OnModelResponse += HandleModelResponse;
            SetPromptActive(false);
        }

        private void HandleModelResponse(string response)
        {
            if (string.IsNullOrEmpty(response)) return;
            var lower = response.ToLowerInvariant();
            if (lower.Contains("goodbye") || lower.Contains("see you") || lower.Contains("bye"))
            {
                EndConversation();
                return;
            }

            // additional reaction logic here
        }

        void EndConversation()
        {
            Unsubscribe();
            isDialogueActive = false;
            onCloseDialogue?.Invoke();
            if (playerInRange) SetPromptActive(true);
        }

        public void Unsubscribe()
        {
            if (dm != null) dm.OnModelResponse -= HandleModelResponse;
        }

        void OnDisable() => Unsubscribe();
        void OnDestroy() => Unsubscribe();

        void OnMouseDown() => TryInteract(); // for editor convenience

        public void SetPortrait(Sprite s) => portrait = s;

        public void SetPlayerInRangeFromForwarder(bool inRange)
        {
            // update internal flag used by CanInteract() and show/hide prompt accordingly
            playerInRange = inRange;
            SetPromptActive(inRange);
        }

        // --- helpers ---
        void EnsureTriggerChildExists()
        {
            // check for an existing trigger child with InteractionForwarder
            foreach (Transform child in transform)
            {
                var bf = child.GetComponent<InteractionForwarder>();
                if (bf != null) return;
            }

            // create new child trigger
            var go = new GameObject("InteractionTrigger");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            var bc = go.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            bc.size = triggerChildSize;

            // add forwarder so InteractionDetector finds an IInteractable on the child
            go.AddComponent<InteractionForwarder>();
        }
    }
}