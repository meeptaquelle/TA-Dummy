using System.Collections.Generic;
using UnityEngine;

public static class NodePathfinding
{
    public static List<Node> FindPath(Node start, Node goal)
    {
        var openSet = new List<Node> { start };
        var cameFrom = new Dictionary<Node, Node>();

        var gScore = new Dictionary<Node, float>();
        var fScore = new Dictionary<Node, float>();

        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            Node current = GetLowestF(openSet, fScore);

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);

            foreach (var neighbor in current.neighbors)
            {
                float penalty = NodeReservationSystem.Instance.GetUsage(neighbor) * 5f;

                float tentativeG = gScore[current]
                    + Vector3.Distance(current.transform.position, neighbor.transform.position)
                    + penalty;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    private static float Heuristic(Node a, Node b)
    {
        return Vector3.Distance(a.transform.position, b.transform.position);
    }

    private static Node GetLowestF(List<Node> set, Dictionary<Node, float> fScore)
    {
        Node best = set[0];
        float bestScore = fScore.ContainsKey(best) ? fScore[best] : float.MaxValue;

        foreach (var n in set)
        {
            float score = fScore.ContainsKey(n) ? fScore[n] : float.MaxValue;
            if (score < bestScore)
            {
                best = n;
                bestScore = score;
            }
        }

        return best;
    }

    private static List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
    {
        List<Node> path = new List<Node> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }
}