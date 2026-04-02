using UnityEngine;
using UnityEngine.AI;

public class ChaserAI : MonoBehaviour
{
    enum State { Patrol, Chase, Search }
    State currentState = State.Patrol;

    public bool isFlanker;
    public float viewAngle = 90f;
    public float viewDistance = 1f;

    int flankerSlotIndex = -1;
    const float orbitRadius = 3.5f;
    const float touchRadius = 0.4f;
    const float attackRadius = 3.7f;
    const float commitRadius = 3.7f;

    bool committed = false;

    NavMeshAgent agent;
    Transform player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (isFlanker)
            flankerSlotIndex = SharedBlackboard.Instance.ClaimFlankerSlot();
    }

    void Update()
    {
        GetComponent<Renderer>().material.color =
            currentState == State.Chase ? Color.red :
            currentState == State.Search ? Color.yellow :
            Color.white;

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (CanSeePlayer())
                {
                    SharedBlackboard.Instance.playerDetected = true;
                    SharedBlackboard.Instance.sharedStamina = 200f;
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                Chase();
                break;

            case State.Search:
                Search();
                break;
        }
    }

    bool CanSeePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle < viewAngle / 2)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, viewDistance))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    SharedBlackboard.Instance.lastSeenPlayerPos = player.position;
                    return true;
                }
            }
        }
        return false;
    }

    void Patrol()
    {
        if (SharedBlackboard.Instance.playerDetected)
            currentState = State.Chase;
    }

    void Chase()
    {
        SharedBlackboard.Instance.sharedStamina -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);
        bool hasLOS = HasDirectLOS();
        Vector3 destination;

        if (isFlanker)
        {
            if (!committed && dist < commitRadius && hasLOS)
                committed = true;

            destination = committed
                ? player.position
                : GetDirectionalApproachTarget(dist);
        }
        else
        {
            destination = player.position;
        }

        agent.SetDestination(destination);

        if (SharedBlackboard.Instance.sharedStamina <= 0)
        {
            SharedBlackboard.Instance.playerDetected = false;
            committed = false;
            currentState = State.Search;
        }
    }

    void Search()
    {
        agent.SetDestination(SharedBlackboard.Instance.lastSeenPlayerPos);

        if (Vector3.Distance(transform.position, SharedBlackboard.Instance.lastSeenPlayerPos) < 1f)
            currentState = State.Patrol;
    }

    Vector3 GetDirectionalApproachTarget(float distToPlayer)
    {
        int totalFlankers = SharedBlackboard.Instance.flankerSlots.Count;
        float angle = flankerSlotIndex * (360f / totalFlankers) + 90f;
        Vector3 slotDir = Quaternion.Euler(0, angle, 0) * player.forward;

        float t = Mathf.Clamp01((distToPlayer - touchRadius) / (attackRadius - touchRadius));
        float scaledRadius = Mathf.Lerp(0f, orbitRadius, t);
        Vector3 rawTarget = player.position + slotDir.normalized * scaledRadius;

        if (NavMesh.SamplePosition(rawTarget, out NavMeshHit hit, orbitRadius, NavMesh.AllAreas))
            if (IsNavMeshReachable(hit.position))
                return hit.position;

        return player.position;
    }

    bool HasDirectLOS()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, commitRadius + 1f))
            return hit.transform.CompareTag("Player");
        return false;
    }

    bool IsNavMeshReachable(Vector3 targetPos)
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(targetPos, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftBoundary * viewDistance);
        Gizmos.DrawRay(transform.position, rightBoundary * viewDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }

    void OnDrawGizmosSelected()
    {
        NavMeshAgent nav = GetComponent<NavMeshAgent>();
        if (nav == null || !nav.hasPath) return;

        Gizmos.color = Color.cyan;
        Vector3[] corners = nav.path.corners;
        for (int i = 0; i < corners.Length - 1; i++)
            Gizmos.DrawLine(corners[i], corners[i + 1]);

        Gizmos.color = Color.blue;
        foreach (var corner in corners)
            Gizmos.DrawSphere(corner, 0.15f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(nav.destination, 0.25f);

        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}