using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachLocationTrigger : MonoBehaviour
{
    [Tooltip("ID of the location to report to the QuestController (e.g., 'School')")]
    public string locationID;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (QuestController.Instance != null)
        {
            Debug.Log($"ReachLocationTrigger: Player entered '{locationID}' trigger.");
            QuestController.Instance.OnLocationReached(locationID);
        }
    }
}
