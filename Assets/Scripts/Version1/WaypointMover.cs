using System.Collections;
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
    public static bool onoff = true;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Store children as waypoints
        int count = waypointParent.childCount;
        waypoints = new Transform[count];
        for (int i = 0; i < count; i++)
            waypoints[i] = waypointParent.GetChild(i);
    }

    void Update()
    {
        if (PauseController.IsGamePaused || isWaiting || !onoff)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        MoveToWaypoint();
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
        animator.SetFloat("InputX", direction.x);
        animator.SetFloat("InputY", direction.y);
        animator.SetBool("isWalking", direction.magnitude > 0.05f);

        // Check arrival
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            if (!isWaiting)  // prevent double coroutine calls
                StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(waitTime);

        // Go to next waypoint
        currentWaypointIndex++;

        if (loopWaypoints)
            currentWaypointIndex %= waypoints.Length;
        else
            currentWaypointIndex = Mathf.Min(currentWaypointIndex, waypoints.Length - 1);

        isWaiting = false;
    }
}
