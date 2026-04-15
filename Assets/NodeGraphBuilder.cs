using UnityEngine;

public class NodeGraphBuilder : MonoBehaviour
{
    public float connectionDistance = 6f;
    public LayerMask obstacleMask;

    private void Start()
    {
        Node[] nodes = FindObjectsByType<Node>(FindObjectsSortMode.None);

        foreach (var node in nodes)
        {
            foreach (var other in nodes)
            {
                if (node == other) continue;

                float dist = Vector3.Distance(node.transform.position, other.transform.position);

                if (dist <= connectionDistance)
                {
                    if (!Physics.Linecast(node.transform.position, other.transform.position, obstacleMask))
                    {
                        if (!node.neighbors.Contains(other))
                            node.neighbors.Add(other);
                    }
                }
            }
        }
    }
}