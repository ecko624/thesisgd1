using UnityEngine;
using UnityEngine.Events;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private GameObject dialoguePanel; // Drag your DialoguePanel here

    [Header("Identity")]
    public string serverPersonality = "aya";  // MUST BE lowercase: aya, mika, sora
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
        if (!playerTransform) Debug.LogError("Player with tag 'Player' not found!");
    }

    void Update()
{
    // Always check distance
    if (!useTrigger)
        UpdateDistanceCheck();

    // If dialogue is active, block interaction but DO NOT check panel state
    if (isDialogueActive)
    {
        SetPromptActive(false);
        return;
    }

    // Allow interaction normally
    if (playerInRange && Input.GetKeyDown(interactionKey))
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
            SetPromptActive(inRange && !isDialogueActive); // Only show prompt when NOT talking
        }
    }

    void SetPromptActive(bool active)
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(active);
    }

    public void Interact() { if (CanInteract()) TryInteract(); }
    public bool CanInteract() => playerInRange && !isDialogueActive && (dialoguePanel == null || !dialoguePanel.activeInHierarchy);

    void TryInteract()
    {
        isDialogueActive = true;
        SetPromptActive(false);

        GameManager.Instance.currentCharacter = serverPersonality;
        dm.StartInteraction(serverPersonality);
        dm.NpcText.text = greeting;
        dm.OpenDialoguePanel();

        dm.OnModelResponse += OnResponse;
        onOpenDialogue?.Invoke();
        GameManager.Instance.TryStartMeetEveryoneQuest(serverPersonality);
    }

    void OnResponse(string text)
    {
        // Optional: auto-close on certain keywords (you can remove if you want)
        if (text.ToLower().Contains("bye") || text.ToLower().Contains("see you"))
            EndConversation();
    }

    public void EndConversation()
{
    if (!isDialogueActive) return;

    isDialogueActive = false;

    dm.OnModelResponse -= OnResponse;

    dm.CloseDialoguePanel();
    dm.ResetConversation();

    onCloseDialogue?.Invoke();

    if (playerInRange)
        SetPromptActive(true);
}


    void OnDisable()
    {
        dm.OnModelResponse -= OnResponse;
    }
}