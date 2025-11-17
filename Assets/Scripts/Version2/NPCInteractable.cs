using UnityEngine;
using UnityEngine.Events;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [Header("Identity")]
    public string personalityKey = "outgoing_class_rep";
    public string serverPersonality = "aya";  // MUST BE: aya, mika, or sora
    public Sprite portrait;
    [TextArea] public string greeting = "Hey!";
    
    [Header("Interaction")]
    public GameObject interactPrompt;
    public KeyCode interactionKey = KeyCode.E;
    public float maxInteractDistance = 5f;
    public bool useTrigger = false;
    public bool addTriggerChildForDetection = true;
    public Vector2 triggerChildSize = new Vector2(1.5f, 2f);

    [Header("Events")]
    public UnityEvent onOpenDialogue;
    public UnityEvent onCloseDialogue;

    private DialogueControllerVersion2 dm;
    private bool playerInRange = false;
    private Transform playerTransform;
    private bool isDialogueActive = false;

    void Awake()
    {
        dm = FindObjectOfType<DialogueControllerVersion2>();
        if (interactPrompt) interactPrompt.SetActive(false);
        if (addTriggerChildForDetection) EnsureTriggerChildExists();
    }

    void Start()
{
    playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    Debug.Log($"[PLAYER DEBUG] Found: {(playerTransform != null ? playerTransform.name : "NULL")}, Tag OK? {GameObject.FindGameObjectWithTag("Player") != null}");
}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
    {
        Debug.Log("[FORCE] E pressed - forcing interaction!");
        TryInteract();
        return;
    }
        if (Input.GetKeyDown(KeyCode.T))  // Press T to test
    {
        Debug.Log($"[DEBUG] playerInRange: {playerInRange}, isDialogueActive: {isDialogueActive}, dm: {(dm != null ? "OK" : "NULL")}");
        TryInteract();
    }
        if (!useTrigger) UpdateDistanceCheck();
        if (!isDialogueActive && playerInRange && Input.GetKeyDown(interactionKey))
            TryInteract();
    }

    void UpdateDistanceCheck()
{
    if (playerTransform == null)
    {
        Debug.LogWarning("[DISTANCE] playerTransform NULL - check Player tag!");
        return;
    }

    float dist = Vector2.Distance(transform.position, playerTransform.position);
    bool inRange = dist <= maxInteractDistance;

    // === REAL-TIME DEBUG ===
    if (Time.frameCount % 60 == 0 || inRange != playerInRange)  // Every second or change
        Debug.Log($"[DISTANCE] {dist:F1}m (Max: {maxInteractDistance}) → InRange: {inRange}");

    if (inRange != playerInRange)
    {
        playerInRange = inRange;
        SetPromptActive(inRange);
    }
}

    void OnTriggerEnter2D(Collider2D c) { if (useTrigger && c.CompareTag("Player")) { playerInRange = true; SetPromptActive(true); } }
    void OnTriggerExit2D(Collider2D c) { if (useTrigger && c.CompareTag("Player")) { playerInRange = false; SetPromptActive(false); } }

    void SetPromptActive(bool active)
{
    if (interactPrompt != null)
        interactPrompt.SetActive(active);
    else if (active && playerInRange)
        Debug.Log("Interact Prompt not assigned on " + gameObject.name);
}

    public void Interact() { if (CanInteract()) TryInteract(); }
    public bool CanInteract() => !isDialogueActive && playerInRange;

    void TryInteract()
    {
        if (dm == null) dm = FindObjectOfType<DialogueControllerVersion2>();
        if (dm == null) return;

        isDialogueActive = true;
        GameManager.Instance.currentCharacter = serverPersonality;
        dm.StartInteraction(serverPersonality);

        if (dm.NpcText != null) dm.NpcText.text = greeting;
        dm.OpenDialoguePanel();

        dm.OnModelResponse += HandleResponse;
        onOpenDialogue?.Invoke();
        SetPromptActive(false);

        GameManager.Instance.TryStartMeetEveryoneQuest(serverPersonality);
    }

    void HandleResponse(string response)
    {
        if (response.ToLower().Contains("bye") || response.ToLower().Contains("see you"))
            EndConversation();
    }

    void EndConversation()
    {
        dm.OnModelResponse -= HandleResponse;
        isDialogueActive = false;
        onCloseDialogue?.Invoke();
        if (playerInRange) SetPromptActive(true);
    }

    void OnDisable() => dm.OnModelResponse -= HandleResponse;
    void OnDestroy() => dm.OnModelResponse -= HandleResponse;
    void OnMouseDown() => TryInteract();

    void EnsureTriggerChildExists()
    {
        if (transform.Find("InteractionTrigger")) return;
        var go = new GameObject("InteractionTrigger");
        go.transform.SetParent(transform, false);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = triggerChildSize;
        go.AddComponent<InteractionForwarder>();
    }
}