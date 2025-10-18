using UnityEngine;

public class InteractionForwarder : MonoBehaviour, IInteractable
{
    private IInteractable parentInteractable;

    void Awake()
    {
        parentInteractable = FindParentInteractable();
        if (parentInteractable == null)
        {
            Debug.LogWarning($"InteractionForwarder on '{gameObject.name}' couldn't find an IInteractable in parents. Parent chain:");
            Transform t = transform.parent;
            while (t != null)
            {
                Debug.Log($" - {t.name} components:");
                var mbs = t.GetComponents<MonoBehaviour>();
                foreach (var mb in mbs) Debug.Log($"    { (mb==null? "MISSING": mb.GetType().FullName) }");
                t = t.parent;
            }
        }
    }

    private IInteractable FindParentInteractable()
    {
        Transform t = transform.parent;
        while (t != null)
        {
            var mbs = t.GetComponents<MonoBehaviour>();
            foreach (var mb in mbs)
            {
                if (mb == null) continue;
                if (mb.GetType() == typeof(InteractionForwarder)) continue;
                if (mb is IInteractable ia) return ia;
            }
            t = t.parent;
        }
        return null;
    }

    // Forward IInteractable calls
    public void Interact() => parentInteractable?.Interact();
    public bool CanInteract() => parentInteractable != null && parentInteractable.CanInteract();

    // Notify parent about player proximity so parent can set its internal playerInRange flag
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        // call parent method if present; DontRequireReceiver to avoid errors
        transform.parent?.gameObject.SendMessage("SetPlayerInRangeFromForwarder", true, SendMessageOptions.DontRequireReceiver);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        transform.parent?.gameObject.SendMessage("SetPlayerInRangeFromForwarder", false, SendMessageOptions.DontRequireReceiver);
    }
}