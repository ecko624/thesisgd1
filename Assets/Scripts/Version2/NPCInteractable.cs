using UnityEngine;
using UnityEngine.Events;
using Version2;

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

    private DialogueControllerVersion2 dm; // Changed from DialogueManager
    private bool playerInRange = false;
    private Transform playerTransform;
    private bool isDialogueActive = false;

    void Awake()
    {
        dm = FindObjectOfType<DialogueControllerVersion2>(); // Changed from DialogueManager
        if (dm == null) Debug.LogWarning("NPCInteractable: DialogueControllerVersion2 not found in scene.");
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
        if (dm == null) dm = FindObjectOfType<DialogueControllerVersion2>(); // Changed from DialogueManager
        if (dm == null) return;

        isDialogueActive = true;

        // Set identity for DialogueControllerVersion2 and GameManager
        dm.StartInteraction(personalityKey, isSingleLine: false); // For grandfather 
        dm.tone = tone; // Sync tone (though currently not used in GetNPCResponse directly)

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentCharacter = personalityKey;
            GameManager.Instance.currentTone = tone; // Sync tone
            // Sync intimacy (assuming GameManager handles this)
            GameManager.Instance.currentIntimacy = GameManager.Instance.GetIntimacyLevel(
                personalityKey == "outgoing_class_rep" ? GameManager.Instance.ayaIntimacy :
                personalityKey == "library_ghost" ? GameManager.Instance.mikaIntimacy : GameManager.Instance.soraIntimacy
            );
        }

        if (dm.npcText != null) dm.npcText.text = greeting;

        // Explicitly open the UI panel
        dm.OpenDialoguePanel();

        // Invoke events and subscribe
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

        // Additional reaction logic here
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

    void OnMouseDown() => TryInteract(); // For editor convenience

    public void SetPortrait(Sprite s) => portrait = s;

    public void SetPlayerInRangeFromForwarder(bool inRange)
    {
        playerInRange = inRange;
        SetPromptActive(inRange);
    }

    void EnsureTriggerChildExists()
    {
        foreach (Transform child in transform)
        {
            var bf = child.GetComponent<InteractionForwarder>();
            if (bf != null) return;
        }

        var go = new GameObject("InteractionTrigger");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        var bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = triggerChildSize;

        go.AddComponent<InteractionForwarder>();
    }
}