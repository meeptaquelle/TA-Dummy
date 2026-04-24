using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Autonomous chaser agent. Coordinates with siblings only via SharedBlackboard.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ChaserAI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Vision")]
    public float viewAngle = 90f;
    public float viewDistance = 15f;

    [Header("Coordination")]
    public float repulsionRadius = 3f;
    public float repulsionStrength = 2f;

    [Header("Flanker")]
    [Tooltip("How far behind the player the flanker tries to approach from")]
    public float flankBehindDistance = 2.5f;
    [Tooltip("How often roles are renegotiated in seconds")]
    public float roleRenegotiateInterval = 2f;
    [Range(-1f, 1f)]
    [Tooltip("Lateral tiebreaker. Set -0.8 on one NPC, 0.8 on the other.")]
    public float personalityBias = 0f;
    public float personalityStrength = 1.2f;

    [Header("Relay")]
    public float staminaRelayThreshold = 5f;

    [Header("Pathfinding")]
    public float recalcInterval = 0.4f;

    // ── Private ───────────────────────────────────────────────────────────────
    enum State { Patrol, Chase, Search }
    State currentState = State.Patrol;

    NavMeshAgent agent;
    Transform player;
    float recalcTimer;
    float roleTimer;
    bool relaySignalPosted;

    SharedBlackboard.AgentRole myRole =>
        SharedBlackboard.Instance.roleRegistry.TryGetValue(this, out var r)
            ? r
            : SharedBlackboard.AgentRole.Rusher;

    static readonly List<ChaserAI> allChasers = new();

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake() => agent = GetComponent<NavMeshAgent>();

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        allChasers.Add(this);
    }

    void OnDestroy()
    {
        allChasers.Remove(this);
        SharedBlackboard.Instance.RemoveAgent(this);
    }

    void Update()
    {
        UpdateDebugColor();

        switch (currentState)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase: UpdateChase(); break;
            case State.Search: UpdateSearch(); break;
        }
    }

    // ── States ────────────────────────────────────────────────────────────────
    void UpdatePatrol()
    {
        if (SharedBlackboard.Instance.playerDetected) { EnterChase(); return; }

        if (CanSeePlayer())
        {
            SharedBlackboard.Instance.playerDetected = true;
            SharedBlackboard.Instance.sharedStamina = 200f;
            EnterChase();
        }
    }

    void EnterChase()
    {
        // Trigger initial role negotiation via blackboard
        SharedBlackboard.Instance.RenegotiateRoles(allChasers, player.position);
        relaySignalPosted = false;
        roleTimer = roleRenegotiateInterval;
        currentState = State.Chase;
    }

    void UpdateChase()
    {
        var bb = SharedBlackboard.Instance;
        bb.sharedStamina -= Time.deltaTime;

        // ── Periodic Role Renegotiation ───────────────────────────────────────
        // Roles shift as agents move — whoever gets closest becomes Rusher.
        // This is dynamic task allocation: no fixed assignments.
        roleTimer -= Time.deltaTime;
        if (roleTimer <= 0f)
        {
            roleTimer = roleRenegotiateInterval;
            bb.RenegotiateRoles(allChasers, player.position);
        }

        // ── Relay Pursuit ─────────────────────────────────────────────────────
        // Exhausted agent posts itself to blackboard.
        // A sibling reads this, promotes itself to Rusher to maintain pressure.
        if (!relaySignalPosted && bb.sharedStamina <= staminaRelayThreshold)
        {
            bb.exhaustedAgent = this;
            relaySignalPosted = true;
        }

        if (bb.exhaustedAgent != null && bb.exhaustedAgent != this)
        {
            bb.roleRegistry[this] = SharedBlackboard.AgentRole.Rusher;
            bb.exhaustedAgent = null;
        }

        // ── Pathfinding ───────────────────────────────────────────────────────
        recalcTimer -= Time.deltaTime;
        if (recalcTimer <= 0f)
        {
            recalcTimer = recalcInterval;
            Vector3 dest = ComputeDestination();
            bb.PostIntent(this, dest);
            agent.SetDestination(dest);
        }

        if (bb.sharedStamina <= 0f)
        {
            bb.playerDetected = false;
            bb.roleRegistry.Clear();
            currentState = State.Search;
        }
    }

    void UpdateSearch()
    {
        agent.SetDestination(SharedBlackboard.Instance.lastSeenPlayerPos);
        if (Vector3.Distance(transform.position, SharedBlackboard.Instance.lastSeenPlayerPos) < 1f)
            currentState = State.Patrol;
    }

    // ── Destination Computation ───────────────────────────────────────────────
    Vector3 ComputeDestination()
    {
        return myRole == SharedBlackboard.AgentRole.Rusher
            ? ComputeRusherDestination()
            : ComputeFlankerDestination();
    }

    /// <summary>
    /// Rusher goes directly at the player with a slight repulsion nudge
    /// to avoid stacking if both agents are somehow assigned Rusher.
    /// </summary>
    Vector3 ComputeRusherDestination()
    {
        Vector3 target = player.position + ComputeSiblingRepulsion() * 0.3f;
        return SampleNavMesh(target, player.position);
    }

    /// <summary>
    /// Flanker approaches from the OPPOSITE side of the Rusher.
    /// 
    /// How it works:
    /// 1. Find the Rusher's current position (read from blackboard intent)
    /// 2. Compute the direction FROM Rusher THROUGH player — that's the flanker's approach vector
    /// 3. Navigate to a point behind the player along that vector
    /// 
    /// This guarantees the two agents approach from opposite sides (pincer),
    /// and the target point is always ON the navmesh near the player — 
    /// not an orbital point that might be behind a wall.
    /// </summary>
    Vector3 ComputeFlankerDestination()
    {
        // Find the Rusher's intended position from the intent board
        Vector3 rusherPos = transform.position; // fallback: use self
        foreach (var entry in SharedBlackboard.Instance.roleRegistry)
        {
            if (entry.Value == SharedBlackboard.AgentRole.Rusher && entry.Key != this)
            {
                // Prefer the Rusher's posted intent, fall back to their transform
                rusherPos = SharedBlackboard.Instance.intentBoard.TryGetValue(entry.Key, out var intent)
                    ? intent
                    : entry.Key.transform.position;
                break;
            }
        }

        // Direction from Rusher through Player = flanker's approach axis
        Vector3 rusherToPlayer = (player.position - rusherPos).normalized;

        // Flanker targets a point behind the player along that axis
        Vector3 flankPoint = player.position + rusherToPlayer * flankBehindDistance;

        // Add personality lateral offset as tiebreaker
        flankPoint += ComputePersonalityOffset();

        // Add sibling repulsion
        flankPoint += ComputeSiblingRepulsion();

        // IMPORTANT: Sample close to player position, not the raw flankPoint.
        // This prevents the agent from targeting a point behind a wall.
        // We try the full flankPoint first, then fall back progressively closer to player.
        return SampleNavMeshProgressive(flankPoint, player.position);
    }

    // ── Supporting Computations ───────────────────────────────────────────────
    Vector3 ComputeSiblingRepulsion()
    {
        Vector3 repulsion = Vector3.zero;
        foreach (ChaserAI sibling in allChasers)
        {
            if (sibling == this) continue;
            Vector3 away = transform.position - sibling.transform.position;
            float dist = away.magnitude;
            if (dist < repulsionRadius && dist > 0.01f)
            {
                float strength = (repulsionRadius - dist) / repulsionRadius;
                repulsion += away.normalized * strength * repulsionStrength;
            }
        }
        return repulsion;
    }

    Vector3 ComputePersonalityOffset()
    {
        Vector3 toPlayer = player.position - transform.position;
        Vector3 lateral = Vector3.Cross(toPlayer.normalized, Vector3.up);
        return lateral * personalityBias * personalityStrength;
    }

    /// <summary>
    /// Tries to find a valid NavMesh point near the preferred target.
    /// If not reachable, steps halfway toward the player and tries again.
    /// This prevents the flanker from ever stopping at an unreachable wall point.
    /// </summary>
    Vector3 SampleNavMeshProgressive(Vector3 preferred, Vector3 fallback)
    {
        // Try at full distance
        if (TryGetReachablePoint(preferred, out Vector3 result))
            return result;

        // Step halfway toward player and try again
        Vector3 midpoint = Vector3.Lerp(preferred, fallback, 0.5f);
        if (TryGetReachablePoint(midpoint, out result))
            return result;

        // Final fallback: go directly to player
        return fallback;
    }

    Vector3 SampleNavMesh(Vector3 preferred, Vector3 fallback)
    {
        return TryGetReachablePoint(preferred, out Vector3 result) ? result : fallback;
    }

    bool TryGetReachablePoint(Vector3 point, out Vector3 result)
    {
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, repulsionRadius + 1f, NavMesh.AllAreas))
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    // ── Vision ────────────────────────────────────────────────────────────────
    bool CanSeePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dir) >= viewAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, viewDistance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                SharedBlackboard.Instance.lastSeenPlayerPos = player.position;
                return true;
            }
        }
        return false;
    }

    // ── Debug ─────────────────────────────────────────────────────────────────
    void UpdateDebugColor()
    {
        if (currentState == State.Chase)
            GetComponent<Renderer>().material.color =
                myRole == SharedBlackboard.AgentRole.Rusher ? Color.red : Color.magenta;
        else
            GetComponent<Renderer>().material.color =
                currentState == State.Search ? Color.yellow : Color.white;
    }

    void OnDrawGizmos()
    {
        // Vision cone
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position,
            Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward * viewDistance);
        Gizmos.DrawRay(transform.position,
            Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward * viewDistance);

        // Repulsion radius
        Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, repulsionRadius);
    }

    void OnDrawGizmosSelected()
    {
        if (agent == null || !agent.hasPath) return;

        // Current path
        Gizmos.color = Color.cyan;
        Vector3[] corners = agent.path.corners;
        for (int i = 0; i < corners.Length - 1; i++)
            Gizmos.DrawLine(corners[i], corners[i + 1]);

        Gizmos.color = Color.blue;
        foreach (var c in corners) Gizmos.DrawSphere(c, 0.15f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(agent.destination, 0.25f);

        // Flanker: draw the approach axis
        if (player != null && myRole == SharedBlackboard.AgentRole.Flanker)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, player.position);
            Gizmos.DrawWireSphere(agent.destination, 0.4f);
        }
    }
}