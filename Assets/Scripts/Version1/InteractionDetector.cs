using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;

    void Start()
    {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && interactableInRange != null)
        {
            if (interactableInRange.CanInteract())
            {
                interactableInRange.Interact();
                interactionIcon.SetActive(false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var forwarder = other.GetComponent<InteractionForwarder>();
        if (forwarder != null)
        {
            interactableInRange = forwarder;
            Debug.Log($"InteractionDetector: found interactableInRange = {forwarder.gameObject.name}");
            if (interactionIcon != null) interactionIcon.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var forwarder = other.GetComponent<InteractionForwarder>();
        if (forwarder != null && interactableInRange == forwarder)
        {
            interactableInRange = null;
            Debug.Log("InteractionDetector: cleared interactableInRange");
            if (interactionIcon != null) interactionIcon.SetActive(false);
        }
    }
}