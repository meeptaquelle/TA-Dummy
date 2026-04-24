using System.Collections.Generic;
using UnityEngine;

public class NodeReservationSystem : MonoBehaviour
{
    public static NodeReservationSystem Instance;

    private Dictionary<Node, int> usageCount = new Dictionary<Node, int>();

    private void Awake()
    {
        Instance = this;
    }

    public int GetUsage(Node node)
    {
        return usageCount.ContainsKey(node) ? usageCount[node] : 0;
    }

    public void ReservePath(List<Node> path)
    {
        foreach (var node in path)
        {
            if (!usageCount.ContainsKey(node))
                usageCount[node] = 0;

            usageCount[node]++;
        }
    }

    public void ReleasePath(List<Node> path)
    {
        foreach (var node in path)
        {
            if (usageCount.ContainsKey(node))
            {
                usageCount[node]--;

                if (usageCount[node] <= 0)
                    usageCount.Remove(node);
            }
        }
    }
}