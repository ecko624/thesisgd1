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

            WaypointMover.onoff = false;

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
            WaypointMover.onoff = true;
            interactableInRange = null;
            Debug.Log("InteractionDetector: cleared interactableInRange");
            if (interactionIcon != null) interactionIcon.SetActive(false);
            if (Application.loadedLevelName=="DIAG4.1")
            {
                Application.LoadLevel("DIAG4");  

            }
            if (Application.loadedLevelName == "DIAG5.1")
            {
                Application.LoadLevel("DIAG6");

            }
            if (Application.loadedLevelName == "DIAG23.1")
            {
                Application.LoadLevel("DIAG23.1A");

            }
            if (Application.loadedLevelName == "DIAG23.3")
            {
                Application.LoadLevel("DIAG23.4");

            }


            if (Application.loadedLevelName == "DIAG32.1")
            {
                Application.LoadLevel("DIAG33");

            }

            if (Application.loadedLevelName == "DIAG34")
            {
                Application.LoadLevel("DIAG35");

            }
        }
    
        
    }
}