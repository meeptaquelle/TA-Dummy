using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> neighbors = new List<Node>();
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        foreach (var n in neighbors)
        {
            Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
}
