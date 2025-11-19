using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    public Transform waypointParent;
    public float speed = 2f;
    public float waitTime = 2f;
    public bool loopWaypoints = true;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private Animator animator;
    private bool hasAnimator = false;
    public static bool onoff = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        hasAnimator = animator != null;
        if (!hasAnimator)
        {
            Debug.LogWarning($"WaypointMover: no Animator found on '{gameObject.name}'. NPC movement will work but animation calls will be skipped.");
        }

        // Validate waypoints parent
        if (waypointParent == null)
        {
            Debug.LogError($"WaypointMover: 'waypointParent' is not set on '{gameObject.name}'. Movement disabled.");
            enabled = false;
            return;
        }

        // Store children as waypoints
        int count = waypointParent.childCount;
        waypoints = new Transform[count];
        for (int i = 0; i < count; i++)
            waypoints[i] = waypointParent.GetChild(i);
        }
    }

    void Update()
    {
        if (PauseController.IsGamePaused || isWaiting)
        {
            if (hasAnimator) animator.SetBool("isWalking", false);
            return;
        }
        if (onoff == true)
        {
            MoveToWaypoint();
        }
    }

    void MoveToWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];

        // Keep Z constant so distance works in 2D
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);

        // Move NPC
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Animation direction
        Vector3 direction = (targetPos - transform.position).normalized;
        if (hasAnimator)
        {
            animator.SetFloat("InputX", direction.x);
            animator.SetFloat("InputY", direction.y);
            animator.SetBool("isWalking", direction.magnitude > 0.05f);
        }

        // Check arrival
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        if (hasAnimator) animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(waitTime);

        currentWaypointIndex = loopWaypoints ? (currentWaypointIndex + 1) % waypoints.Length : Mathf.Min(currentWaypointIndex + 1, waypoints.Length - 1);

        isWaiting = false;
    }
}
