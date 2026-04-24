using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Blackboard Pattern — shared coordination medium for all agents.
/// Agents read and write here. No agent holds a direct reference to another.
/// </summary>
public class SharedBlackboard : MonoBehaviour
{
    public static SharedBlackboard Instance;

    [Header("Detection")]
    public bool playerDetected;
    public Vector3 lastSeenPlayerPos;

    [Header("Stamina")]
    public float sharedStamina = 30f;

    // ── Role Registry ─────────────────────────────────────────────────────────
    public enum AgentRole { Rusher, Flanker }
    public Dictionary<ChaserAI, AgentRole> roleRegistry = new();

    // ── Intent Board ──────────────────────────────────────────────────────────
    // Agents post intended destinations so siblings can read and react.
    public Dictionary<ChaserAI, Vector3> intentBoard = new();

    // ── Relay ─────────────────────────────────────────────────────────────────
    public ChaserAI exhaustedAgent;

    void Awake() => Instance = this;

    /// <summary>
    /// Assigns roles based on distance to player.
    /// Closest agent = Rusher, furthest = Flanker.
    /// Called every few seconds so roles shift naturally as positions change.
    /// </summary>
    public void RenegotiateRoles(List<ChaserAI> agents, Vector3 playerPos)
    {
        if (agents.Count == 0) return;

        ChaserAI closest = null;
        float closestDist = float.MaxValue;

        foreach (var agent in agents)
        {
            float dist = Vector3.Distance(agent.transform.position, playerPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = agent;
            }
        }

        foreach (var agent in agents)
            roleRegistry[agent] = (agent == closest)
                ? AgentRole.Rusher
                : AgentRole.Flanker;
    }

    public void PostIntent(ChaserAI agent, Vector3 pos) => intentBoard[agent] = pos;

    public void RemoveAgent(ChaserAI agent)
    {
        roleRegistry.Remove(agent);
        intentBoard.Remove(agent);
        if (exhaustedAgent == agent) exhaustedAgent = null;
    }
}