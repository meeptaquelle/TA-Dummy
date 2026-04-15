using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Node[] patrolNodes;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float stopAtDistance = 0.5f;
    [SerializeField] private float repathInterval = 2f;

    private NavMeshAgent _agent;

    private int _currentPatrolIndex;
    private bool _isWaiting;

    private List<Node> currentPath;
    private int pathIndex;

    private bool isChasing;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(RepathRoutine());
        GoToNextPatrolPoint();
    }

    private void Update()
    {
        if (isChasing)
            FollowPath();
        else
            Patrol();
    }

    // ================= PATROL =================
    private void Patrol()
    {
        if (_isWaiting) return;

        if (!_agent.pathPending && _agent.remainingDistance <= stopAtDistance)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        _isWaiting = true;
        _agent.isStopped = true;

        yield return new WaitForSeconds(patrolWaitTime);

        _agent.isStopped = false;
        GoToNextPatrolPoint();
        _isWaiting = false;
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolNodes.Length == 0) return;

        _agent.SetDestination(patrolNodes[_currentPatrolIndex].transform.position);
        _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolNodes.Length;
    }

    // ================= CHASE =================
    private IEnumerator RepathRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(repathInterval);

            if (player == null) continue;

            isChasing = true;

            Node start = GetClosestNode(transform.position);
            Node goal = GetClosestNode(player.position);

            if (start == null || goal == null) continue;

            if (currentPath != null)
                NodeReservationSystem.Instance.ReleasePath(currentPath);

            currentPath = NodePathfinding.FindPath(start, goal);

            if (currentPath != null)
            {
                NodeReservationSystem.Instance.ReservePath(currentPath);
                pathIndex = 0;
            }
        }
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        Node targetNode = currentPath[pathIndex];

        _agent.SetDestination(targetNode.transform.position);

        if (!_agent.pathPending && _agent.remainingDistance <= stopAtDistance)
        {
            pathIndex++;

            if (pathIndex >= currentPath.Count)
            {
                NodeReservationSystem.Instance.ReleasePath(currentPath);
                currentPath = null;
            }
        }
    }

    private Node GetClosestNode(Vector3 position)
    {
        Node[] allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);

        Node best = null;
        float bestDist = Mathf.Infinity;

        foreach (var node in allNodes)
        {
            float dist = Vector3.Distance(position, node.transform.position);

            if (dist < bestDist)
            {
                best = node;
                bestDist = dist;
            }
        }

        return best;
    }
}