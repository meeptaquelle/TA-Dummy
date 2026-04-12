using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FSM : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Chase,
        Search,
        Attack,
        Stunned
    }

    private State currentState = State.Patrol;

    [Header("References")]
    public Transform[] patrolPoints;
    public Transform player;

    private NavMeshAgent agent;

    [Header("Patrol Settings")]
    public float patrolWaitTime = 2f;
    public float stopDistance = 0.5f;

    private int patrolIndex;
    private bool isWaiting;

    [Header("Vision Settings")]
    public float viewAngle = 90f;
    public float viewDistance = 10f;

    [Header("Chase Settings")]
    public float stamina = 10f;

    private Vector3 lastSeenPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase: UpdateChase(); break;
            case State.Search: UpdateSearch(); break;
            case State.Attack: UpdateAttack(); break;
            case State.Stunned: UpdateStunned(); break;
        }
    }

    void TransitionTo(State newState)
    {
        currentState = newState;
    }

    // =====================
    // PATROL
    // =====================
    void UpdatePatrol()
    {
        if (CanSeePlayer())
        {
            stamina = 200f;
            TransitionTo(State.Chase);
            return;
        }

        if (isWaiting) return;

        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
        {
            StartCoroutine(WaitAtPoint());
        }
    }

    IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(patrolWaitTime);

        agent.isStopped = false;
        GoToNextPoint();
        isWaiting = false;
    }

    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[patrolIndex].position);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    // =====================
    // CHASE
    // =====================
    void UpdateChase()
    {
        stamina -= Time.deltaTime;

        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) < 2f)
        {
            TransitionTo(State.Attack);
            return;
        }

        if (stamina <= 0)
        {
            TransitionTo(State.Search);
        }
    }

    // =====================
    // SEARCH
    // =====================
    void UpdateSearch()
    {
        agent.SetDestination(lastSeenPosition);

        if (Vector3.Distance(transform.position, lastSeenPosition) < 1f)
        {
            TransitionTo(State.Patrol);
        }

        if (CanSeePlayer())
        {
            TransitionTo(State.Chase);
        }
    }

    // =====================
    // ATTACK
    // =====================
    void UpdateAttack()
    {
        agent.SetDestination(transform.position); // stop

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > 2.5f)
        {
            TransitionTo(State.Chase);
        }

        // attack logic here later
    }

    // =====================
    // STUNNED
    // =====================
    void UpdateStunned()
    {
        agent.isStopped = true;

        // recover logic later
    }

    // =====================
    // VISION
    // =====================
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
                    lastSeenPosition = player.position;
                    return true;
                }
            }
        }
        return false;
    }
}