using UnityEngine;
using UnityEngine.Events;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [Header("Identity")]
    public string serverPersonality = "aya";  // MUST BE: aya, mika, sora
    [TextArea] public string greeting = "Hey!";

    [Header("Interaction")]
    public GameObject interactPrompt;
    public KeyCode interactionKey = KeyCode.E;
    public float maxInteractDistance = 3f;
    public bool useTrigger = false;

    [Header("Events")]
    public UnityEvent onOpenDialogue;
    public UnityEvent onCloseDialogue;

    private DialogueControllerVersion2 dm;
    private Transform playerTransform;
    private bool playerInRange = false;
    private bool isDialogueActive = false;

    void Awake()
    {
        dm = FindObjectOfType<DialogueControllerVersion2>();
        if (interactPrompt) interactPrompt.SetActive(false);
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!playerTransform) Debug.LogError("Player tag missing!");
    }

    void Update()
    {
        if (!useTrigger) UpdateDistanceCheck();
        if (!isDialogueActive && playerInRange && Input.GetKeyDown(interactionKey))
            TryInteract();
    }

    void UpdateDistanceCheck()
    {
        if (!playerTransform) return;
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= maxInteractDistance;
        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            SetPromptActive(inRange);
        }
    }

    void SetPromptActive(bool active)
{
    if (interactPrompt != null)
        interactPrompt.SetActive(active);
    else
        Debug.LogWarning($"No interactPrompt assigned on {gameObject.name}");
}

    public void Interact() { if (CanInteract()) TryInteract(); }
    public bool CanInteract() => !isDialogueActive && playerInRange;

    void TryInteract()
    {
        if (dm == null) return;
        isDialogueActive = true;

        GameManager.Instance.currentCharacter = serverPersonality;
        dm.StartInteraction(serverPersonality);
        if (dm.NpcText) dm.NpcText.text = greeting;
        dm.OpenDialoguePanel();

        dm.OnModelResponse += OnResponse;
        onOpenDialogue?.Invoke();
        SetPromptActive(false);
        GameManager.Instance.TryStartMeetEveryoneQuest(serverPersonality);
    }

    void OnResponse(string text)
    {
        if (text.ToLower().Contains("bye") || text.ToLower().Contains("see you"))
            EndConversation();
    }

    void EndConversation()
{
    dm.OnModelResponse -= OnResponse;
    isDialogueActive = false;
    onCloseDialogue?.Invoke();
    
    // ← THIS IS THE MISSING FIX
    SetPromptActive(true);  // Prompt reappears so you can press E again
}

    void OnDisable() => dm.OnModelResponse -= OnResponse;
}